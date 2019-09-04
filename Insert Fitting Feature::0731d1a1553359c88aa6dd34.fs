FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");
shared::import(path : "513f6e32bda69f3ba7d654fd", version : "0a7b9c2c95b93fa0ecfe36de");

enum LogLevels
{
    INFO,
    DEBUG,
    WARN,
    ERROR,
    PERFORMANCE
}

const LogLevel = LogLevels.ERROR;

annotation { "Feature Type Name" : "Insert Fittings" }
export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Fitting Points", "Filter" : EntityType.VERTEX}
        definition.fittingPoints is Query;
        
        annotation { "Name" : "Allow Custom Fittings" }
        definition.allowCustomFittings is boolean;
        
        annotation { "Name" : "Fitting Studio" }
        definition.fittingStudio is PartStudioData;
        
        annotation { "Name" : "Fitting Axes to Enum Key", "Default": "axesToFittingType"}
        definition.axesToFittingTypeKey is string;
        
    }
    {
        insertFittingMateConnectors(context, id, definition);
    });
    
/**
 * definition.pipeLine is Query
 * definition.allowCustomFittings is boolean
 **/
export function insertFittingMateConnectors(context is Context, id is Id, definition is map) returns map
{
    println("The fitting Studio: " ~ definition.fittingStudio);
    const fittingLookupTable = getVariable(definition.fittingStudio.buildFunction(definition.fittingStudio.configuration), 'axesToFittingType');
    var entityByPartId = {};
    var mateConnectors = [];
    var confidences = [];
    const pointQs = evaluateQuery(context, definition.fittingPoints);
    for (var i = 0; i < size(pointQs); i += 1)
    {
        const point = pointQs[i];
        const fittingTransformInfo = getBestFitting(context, id, {'point': point, 'allowCustomFittings': definition.allowCustomFittings, 'fittingLookupTable': fittingLookupTable});
        confidences = append(confidences, fittingTransformInfo.confidence);
        const fittingType = fittingTransformInfo.fittingType;
        const fittingEntry = fittingLookupTable[fittingType];
        log("Final transform: " ~ fittingTransformInfo);
        log("Fitting entry: " ~ fittingEntry);
        const mateConnectorId = id + ("mateConnector"~i);
        opMateConnector(context, mateConnectorId, {
                "coordSystem" : fittingTransformInfo.coordinateSystem,
                "owner" : definition.ownerPart
        });
        const mateConnectorQ = qCreatedBy(mateConnectorId, EntityType.VERTEX);
        var applyToMateConnectors = [];
        setAttribute(context, {
                "entities" : mateConnectorQ,
                "attribute" : {"configuration": {"fittingType" : fittingType}, 
                               "path": "8a566d1945a46e4467b57316", 
                               "type" : "partInstantiationInfo",
                               "inserterType" : "fittingInserter",
                               'lineQueries' : fittingTransformInfo.lineQueries}
        });
        mateConnectors =  append(mateConnectors, mateConnectorQ);
        
    }
    log("entityByPartId: " ~ entityByPartId, LogLevels.DEBUG);
    for (var entry in entityByPartId)
    {
        var key = entry.key;
        var value = entry.value;
        const query = qUnion(value);
        setProperty(context, {
                "entities" : query,
                "propertyType" : PropertyType.NAME,
                "value" : key
        });
    }
    return {'mateConnectors' : qUnion(mateConnectors), 'confidences' : confidences};
}

/**
 * Determine the best fitting from the list of options, and return both the fitting type and the rotation tansform.
 * transform: The linear transformation needed to place the fitting into the world coordinate system
 * fittingType: The type of fitting that should be used
 **/
function getBestFitting(context is Context, id is Id, definition is map) returns map{
    const point = definition.point;
    var bestSoFar = {"confidence": {normalizedConfidence: 0}};
    const fittingGeometry = getObservationVectors(context, evVertexPoint(context, {
        "vertex" : point
    }));
    const n_edges = size(fittingGeometry.axes);
    for (var fitting in definition.fittingLookupTable)
    {
        var fittingType = fitting.value;
        var fittingAxes = fitting.key;
        log("Trying fitting: " ~ fittingType);
        
        // Ignore fittings with the wrong number of vectors
        if (n_edges != size(fittingAxes)){
            continue;
        }

        const permutations = makePermutationsWithMapping(fittingGeometry.axes);
        log("Permutations returned: " ~ permutations);
        for (var permutation in permutations) 
        {
            const davenportInfo = solveDavenportQ(onesVector(n_edges), permutation.permutation as Matrix, fittingAxes as Matrix);
            if (davenportInfo.confidence.normalizedConfidence > bestSoFar.confidence.normalizedConfidence)
            {
                bestSoFar.fittingType = fittingType;
                bestSoFar.coordinateSystem = coordSystem(fittingGeometry.origin, davenportInfo.xAxis, davenportInfo.zAxis);
                bestSoFar.transform = transform(davenportInfo.transform.linear, fittingGeometry.origin);
                bestSoFar.confidence = davenportInfo.confidence;
                bestSoFar.lineQueries = makeSpecificPermutation(fittingGeometry.lineQueries, permutation.iByOriginal);
            }
        }
    }
    
    return bestSoFar;
}

/**
 * origin: the origin of the fitting.
 * axes: a matrix where each row is a vector coming out from the point.
 **/
function getObservationVectors(context is Context, point is Vector) returns map
{
    var edges = evaluateQuery(context, qWithinRadius(qBodyType(qEverything(EntityType.EDGE), BodyType.WIRE), point, TOLERANCE.zeroLength*meter));
    const nAxes = size(edges);
    // log("Point coordinates: " ~ point);
    log("Number of axes: " ~ nAxes);
    var axes is Matrix = zeroMatrix(nAxes, 3);
    for (var i = 0; i < nAxes; i += 1)
    {
        const edge = edges[i];
        var eLine is Line = evLine(context, {
                "edge" : edge
        });
        // log("Point coordinate: " ~ eLine.origin);
        const fittingAxis = tolerantEquals(eLine.origin, point) ? eLine.direction : eLine.origin-point;
        log("FittingAxis: " ~ fittingAxis/meter);
        axes[i] = normalize(fittingAxis);
        
    }
    log("axes matrix: " ~ axes);
    return {"axes": axes, origin: point, 'lineQueries': edges};
}

/**
 * From two arrays of vector observations and their corresponding weights, determine the best rotation transformation
 * from https://en.wikipedia.org/wiki/Wahba%27s_problem
 * transform: the rotational transform needed to change the known into the best 
 **/
function solveDavenportQ(weights is array, measured is array, known is array) returns map
{
    const n = size(weights);
    if (!(n==size(measured) && n==size(known)))
    {
        throw regenError("Weights, measured and known need to be the same size. Currently they are: "~ n ~ " , " ~ size(measured) ~ " , " ~ size(known));
    }
    var B is Matrix = zeroMatrix(3, 3);
    for (var i = 0; i < n; i += 1)
    {
        var m = measured[i];
        var k = known[i];
        B = B + weights[i]*outerPoduct(m, k);
    }
    log("B From the DavenPort method: " ~ B);
    const svdB = svd(B);
    log("Singular Value Decomposition: " ~ svdB, LogLevels.INFO);
    var M = identityMatrix(3);
    const determinantSquared = -determinant(svdB.u)*determinant(transpose(svdB.v));
    log("determinant squared: " ~ determinantSquared);
    M[2] = vector([0,0,-round(determinantSquared)]);
    const result = svdB.u*M*transpose(svdB.v);
    log("Davenport result: " ~ result);
    const resultTransform = transform(result, vector([0,0,0])*meter);
    const transformedFitting = measured*result;
    log("Transformed fitting: " ~ transformedFitting);
    return {"transform" : resultTransform, "confidence": confidence(known, transformedFitting), 'xAxis': result*vector([1,0,0]), 'yAxis': result*vector([0,1,0]), 'zAxis': result*vector([0,0,1])};
}

// Label the edge representing a pipe with the id. If it is already labeled, add the second mateConnector as the other end of the pipe.
function labelPipes(context is Context, id is Id, edges is array)
{
    for (var i = 0; i < size(edges); i += 1)
    {
        const edge = edges[i];
        var partInstantiationInfoAlreadyThere = shared::getAttributeType(context, edge, 'partInstantiationInfo');
        if (partInstantiationInfoAlreadyThere == undefined)
        {
            setAttribute(context, {
                    "entities" : edge,
                    "attribute" : {'type': 'partInstantiationInfo', 'inserterType': 'Pipe Inserter', 'id': id + ('pipeId'~i), 'pipeEnds': [id]}
            });
        } else 
        {
            removeAttributes(context, {'entities': edge, 'attributePattern' : {'type': 'partInstantiationInfo'}});
            partInstantiationInfoAlreadyThere['pipeEnds'] = append(partInstantiationInfoAlreadyThere['pipeEnds'], id);
            setAttribute(context, {
                "entities" : edge,
                "attribute" : partInstantiationInfoAlreadyThere
            });
        }
    }
}


function outerPoduct(u is array, v is array) returns Matrix
{
    var m1 = zeroMatrix(size(u), 1);
    var m2 = zeroMatrix(1, size(v));
    for (var i = 0; i < size(u); i += 1)
    {
        m1[i][0] = u[i];
        m2[0][i] = v[i];
    }
    log("outerProduct response: "~ m1 ~ m2 ~ m1*m2);
    return m1*m2 as Matrix;
}

// The normalized confidence between two Matrices where each row represents a direction vector. Confidence of 1 means all vectors match!
function confidence(m1 is Matrix, m2 is Matrix) {
    log("Confidence is: "~m1~m2);
    const n = size(m1);
    var sum = 0;
    var dots = [];
    for (var i = 0; i < size(m1); i += 1)
    {
        // Surround with try for when normalizing a [0,0,0] vector would return a NaN
        try
        {
            const v1 = normalize(m1[i] as Vector);
            const v2 = normalize(m2[i] as Vector);
            // This is a guess at the best way to provide the confidence... is this a good route?
            const dot = dot(v1, v2);
            dots = append(dots, dot);
            sum += dot;
        }
    }
    return {normalizedConfidence: sum/n, maxDotProduct: max(dots)};
}

// Make all permutations of the array and include the iByOriginal map
function makePermutationsWithMapping(m is array) returns array
{
    const n = size(m);
    var result = [];
    var indexArray = [];
    for (var i = 0; i < n; i += 1)
    {
        indexArray = append(indexArray, i);
    }
    log("indexArray: " ~ indexArray ~ " array: " ~ m);
    if (size(indexArray) != size(m))
    {
        log("Different sizes!");
    }
    for (var iArray in makePermutations(indexArray))
    {
        result = append(result, {iByOriginal: iArray, 'permutation': makeSpecificPermutation(m, iArray)});
    }
    return result;
}

// arrange the passed in array by the passed in index array
function makeSpecificPermutation(a is array, iArray is array) returns array
{
    if (size(a) != size(iArray))
    {
        throw regenError("Don't pass different sizes into makeSpecificPermutation. Size of array: " ~ size(a) ~ " and size of index array: " ~ size(iArray));
    }
    var permutation = [];
    for (var i in iArray)
    {
        permutation = append(permutation, a[i]);
    }
    return permutation;
}

// Make all the different permutations of the array - returns an array of the permuted arrays
function makePermutations(m is array) returns array
{
    const n = size(m);
    if (n==0)
    {
        return [];
    }
    if (n==1)
    {
        return [m];
    }
    var l = [];
    for (var i = 0; i < n; i += 1)
    {
        const v = m[i];
        var remLst = concatenateArrays([subArray(m,0,i), subArray(m,i+1,n)]);
        for (var p in makePermutations(remLst))
        {
            l = append(l,append(p, v));
        }
    }
    return l;
}

function onesVector(nEntries is number){
    var l = [];
    for (var i = 0; i < nEntries; i += 1)
    {
        l = append(l, 1);
    }
    return l;
}

function log(message is string, level is LogLevels)
{
    if (level == LogLevel)
    {
        println(message);
    }
}

function log(message is string)
{
    log(message, LogLevels.INFO);
}
