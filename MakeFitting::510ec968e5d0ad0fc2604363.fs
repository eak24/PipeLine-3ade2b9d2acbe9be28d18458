FeatureScript 1112;
import(path : "onshape/std/geometry.fs", version : "1112.0");
shared::import(path : "513f6e32bda69f3ba7d654fd", version : "0a7b9c2c95b93fa0ecfe36de");


annotation { "Feature Type Name" : "Fitting" }
export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        // Define the parameters of the feature type
        annotation { "Name" : "partId" }
        isAnything(definition.partId);
        
        annotation { "Name" : "Fitting Type" }
        definition.fittingType is FittingType;
        
        annotation { "Name" : "Custom Fitting Axes" }
        isAnything(definition.customFittingAxes);
        
        annotation { "Name" : "Fitting Thickness" }
        isLength(definition.pipeThickness, { (inch) : [0, .125, 1e5] } as LengthBoundSpec);
    
        annotation { "Name" : "Socket Depth" }
        isLength(definition.socketDepth, { (inch) : [0, .125, 1e5] } as LengthBoundSpec);
        
        annotation { "Name" : "Axis Length" }
        isLength(definition.axisLength, { (inch) : [0, .125, 1e5] } as LengthBoundSpec);
        
        annotation { "Name" : "Pipe OD" }
        isLength(definition.od, { (inch) : [0, .125, 1e5] } as LengthBoundSpec);
        
        annotation { "Name" : "Nonuniform Dimensions" }
        definition.nonuniformDimensions is boolean;
        
        if (definition.nonuniformDimensions) 
        {
            annotation { "Name" : "Axes Dimensions", "Item name" : "Axis Dimensions" }
            definition.nonuniformDimensionsArray is array;
            for (var widget in definition.nonuniformDimensionsArray)
            {
                annotation { "Name" : "Axis Number" }
                isInteger(widget.axisNumber, { (unitless) : [0, 1, 1e5] } as IntegerBoundSpec);
                
                annotation { "Name" : "Socket Depth" }
                isLength(widget.wsocketDepth, LENGTH_BOUNDS);
                
                annotation { "Name" : "Axis Length" }
                isLength(widget.waxisLength, LENGTH_BOUNDS);
                
                annotation { "Name" : "Pipe OD" }
                isLength(widget.wod, LENGTH_BOUNDS);
            }
        }
    }
    {
        // Export the table
        setVariable(context, "axesToFittingType", axesToFittingType);
        if (definition.nonuniformDimensions)
        {
            // Prepare inputs
            const withNonuniformDimensions = assignNonuniformDimensions({
                "socketDepth": definition.socketDepth,
                "axisLength": definition.axisLength,
                "od": definition.od}, 
                definition.nonuniformDimensionsArray, "axisNumber", "w");
            
            definition = mergeMaps(definition, withNonuniformDimensions);
        }
        println('definition: ' ~ definition);
        definition.axes = getFittingAxes(definition.fittingType, definition.customFittingAxes);
        const fittingQ = makeFittingGeometry(context, id, definition);
        setProperty(context, {
                "entities" : fittingQ,
                "propertyType" : PropertyType.NAME,
                "value" : definition.partId
        });
    });
    
/**
 * Organizes data that looks like: 
 * defaultValues: {a:1, b:2, c:3}
 * nonuniformValues: [{innerKey:2, k_b:5, k_c:15}]
 * arrayKeyPrefix: k_
 * innerKey: innerKey 
 * result: {a:{"Default": 1}, b:{"Default": 2, 2:5}, c:{"Default": 3, 2:15}}
 **/
function assignNonuniformDimensions(defaultValues is map, nonuniformValues is array, innerKey is string, arrayKeyPrefix is string) returns map
{
    var result = {};
    for (var defaultValue in defaultValues)
    {
        var nonUniformWithDefault = {"Default" : defaultValue.value};
        for (var nonuniformValue in nonuniformValues)
        {
            const key = nonuniformValue[innerKey];
            nonUniformWithDefault[key] = nonuniformValue[arrayKeyPrefix ~ defaultValue.key];
        }
        result[defaultValue.key] = nonUniformWithDefault;
    }
    return result;
}
    
export function makeFittingGeometry(context, id, fittingInfo is map) {
    const n = size(fittingInfo.axes);
    var openings = [];
    var axisBodies = [];
    for (var i = 0; i < n; i += 1)
    {
        const definition = {axis: fittingInfo.axes[i],
            od : extractValue(fittingInfo.od, i),
            thickness : extractValue(fittingInfo.pipeThickness, i),
            socketDepth : extractValue(fittingInfo.socketDepth, i),
            axisLength : extractValue(fittingInfo.axisLength, i)};
        println('axis definition for ' ~ i ~ ": " ~ fittingInfo.axes[i]);
        const result = makeFittingAxis(context, id+i, definition);
        openings = append(openings, result.surfaceToShell);
        axisBodies = append(axisBodies, result.axisBody);
    }
    if (n>1)
    {
        opBoolean(context, id + "boolean1", {
            "tools" : qUnion(axisBodies),
            "operationType" : BooleanOperationType.UNION,
            "keepTools" : true
        });
    }
    var toShell = [];
    const fittingQ = qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID);
    for (var i = 0; i < n; i += 1)
    {
        const extrudeId = id + ("extrude"~i);
        extrude(context, extrudeId, {
            "entities" : openings[i],
            "endBound" : BoundingType.BLIND,
            "depth" : -extractValue(fittingInfo.socketDepth, i),
            "defaultScope" : false,
            "booleanScope" : fittingQ,
            operationType : NewBodyOperationType.REMOVE
        });
        const mcFace = evaluateQuery(context, qCreatedBy(extrudeId, EntityType.FACE))[1];
        toShell = append(toShell, qCreatedBy(extrudeId, EntityType.FACE));
        const plane = evPlane(context, {
                "face" : mcFace
        });
        const mateConnectorId = id + ("mateConnector" ~ i);
        opMateConnector(context, mateConnectorId, {
                "coordSystem" : coordSystem(plane),
                "owner" : fittingQ
        });
        setAttribute(context, {
                "entities" : qCreatedBy(mateConnectorId, EntityType.VERTEX),
                "attribute" : {'type': 'fittingAxisMcId', 'id': i}
        });
    }
    opShell(context, id + "shell1", {
        "entities" : qUnion(toShell),
        "thickness" : -fittingInfo.pipeThickness*2
    });
    opDeleteBodies(context, id + "deleteBodies1", {
        "entities" : qSketchFilter(qUnion([qCreatedBy(id, EntityType.VERTEX), qCreatedBy(id, EntityType.EDGE)]), SketchObject.YES)
    });
    setAttribute(context, {
            "entities" : fittingQ,
            "attribute" : fittingInfo
    });
    return fittingQ;
}

function extractValue(field, i)
{
    if (isLength(field, NONNEGATIVE_LENGTH_BOUNDS))
    {
        return field;
    }
    else if (field is map)
    {
        if (field[i] != undefined)
        {
            println("yodles:" ~field[i]);
            return field[i];
        }
        else
        {
            return field["Default"];
        }
    }
    else if (field is array)
    {
        return field[i];
    }
    return field;
}

/**
 * @param definition {{
 *      @field axis {Vector} A vector representing the direction in which to draw the axis.
 *      @field od {Length} The real outer diameter of the pipe inserted into the fitting
 *      @field thickness {Length} The wall thickness of the pipe inserted into the fitting
 *      @field socketDepth {Length} The socket depth
 *      @field axisLength {Length} The length from the center vertex of the fitting to the edge.
 * }}
 * Returns map:
 * @returns {{
 *      @field surfaceToShell
 * }}
 **/
function makeFittingAxis(context, id, definition is map) returns map {
    println(definition.axis);
    const vIn = definition.axis as Vector;
    
    const od = definition.od;
    const thickness = definition.thickness;
    const socketDepth = definition.socketDepth;
    const axisLength = definition.axisLength;
    const socketRadius = od/2;
    const outerRadius = socketRadius + thickness;
    
    const lineResult = line(vector([0,0,0])*inch,vIn);
    println(lineResult);
    var baseNormal = perpendicularVector(lineResult.direction);
    var normal = rotationMatrix3d(lineResult.direction, 90*degree) * baseNormal;
    println(normal);
    const planeResult = plane(lineResult.origin, normal, lineResult.direction);
    
    const sketch1 = newSketchOnPlane(context, id + "sketch1", {
            "sketchPlane" : planeResult
    });
    skLineSegment(sketch1, "line1", {
            "start" : vector(0, 0) * inch,
            "end" : worldToPlane(planeResult, normalize(vIn)*axisLength),
            "construction" : true
    });
    skLineSegment(sketch1, "line2", {
            "start" : vector(0*inch, outerRadius),
            "end" : vector(axisLength, outerRadius)
    });
    const line3 = skLineSegment(sketch1, "line3", {
            "start" : vector(axisLength, outerRadius),
            "end" : vector(axisLength, 0*inch)
    });
    const line4 = skLineSegment(sketch1, "line4", {
            "start" : vector(axisLength, 0*inch),
            "end" : vector(-outerRadius, 0*inch)
    });
    const middleDistance = sin(45*degree)*outerRadius;
    skArc(sketch1, "line5", {
            "start" : vector(-outerRadius, 0*inch),
            "mid" : vector(-middleDistance, middleDistance),
            "end" : vector(0*inch, outerRadius)
    });
    skSolve(sketch1);
    opRevolve(context, id + "revolve1", {
            "entities" : qCreatedBy(id + "sketch1", EntityType.FACE),
            "axis" : lineResult,
            "angleForward" : 360 * degree
    });
    const axisBody = qCreatedBy(id + "revolve1", EntityType.BODY);
    const fittingMouth = qWithinRadius(qCreatedBy(id + "revolve1", EntityType.FACE), normalize(vIn)*axisLength, 10e-8*meter);
    const sketch2 = newSketch(context, id + "sketch2", {
            "sketchPlane" : fittingMouth
    });
    const circle1 = skCircle(sketch2, "circle1", {
            "center" : vector(0, 0) * inch,
            "radius" : socketRadius
    });
    skSolve(sketch2);
    const socketMouth = qSketchRegion(id + "sketch2");
    return {surfaceToShell: socketMouth, "axisBody": axisBody};
}

export enum FittingType
{
    CAP,
    CROSS,
    CORNER,
    CUSTOM,
    ELBOW_22_5,
    ELBOW_45,
    ELBOW_90,
    TEE,
    STRAIGHT,
    WYE_45
}

const fittingTypeToAxes = {
    FittingType.CAP : [[1,0,0]],
    FittingType.CORNER : [[1,0,0],[0,1,0],[0,0,1]],
    FittingType.CROSS : [[-1,0,0],[1,0,0],[0,1,0],[0,-1,0]],
    FittingType.ELBOW_22_5 : [[1,0,0],[-1,0.48717451246,0]],
    FittingType.ELBOW_45 : [[1,0,0],[-1,1,0]],
    FittingType.ELBOW_90 : [[1,0,0],[0,-1,0]],
    FittingType.TEE : [[-1,0,0],[1,0,0],[0,1,0]],
    FittingType.STRAIGHT : [[-1,0,0],[1,0,0]],
    FittingType.WYE_45 : [[-1,0,0],[1,1,0],[1,0,0]]
    };
    
export const axesToFittingType = shared::invertMap(fittingTypeToAxes);

function getFittingAxes(fittingType is FittingType, customAxes is string)
{
    var result;
    if (fittingType == FittingType.CUSTOM) 
    {
        result = parseJson(customAxes);
    }
    else
    {
        result = fittingTypeToAxes[fittingType];
    }
    println(fittingType);
    return result;
}