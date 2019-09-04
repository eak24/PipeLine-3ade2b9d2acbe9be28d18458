FeatureScript 1112;
import(path : "onshape/std/geometry.fs", version : "1112.0");
insertFitting::import(path : "0731d1a1553359c88aa6dd34", version : "246bc55e2b1e889cf66e00e9");
mcInstantiator::import(path : "555d1efb77a4fff94fe88228", version : "3d18fd39ceea915637b2784e");
shared::import(path : "513f6e32bda69f3ba7d654fd", version : "0a7b9c2c95b93fa0ecfe36de");
icon::import(path : "9d17bb24186667140786f1e9", version : "b164c3463cdd4e37929cad35");
fittingStudio::import(path : "58a225300a6bf030f2fa4f20", version : "ee6e0994071446527d2d1325");
pipeStudio::import(path : "43d1b35204cbd7a24a9c97f7", version : "67d3f8b07c1846f901de5b14");
modifiedInstantiator::import(path : "c81fce53dede81ef89860aa3/4a701265a13057eef3bface7/b453944163f91ccf1477e3f0", version : "2b4306d8bcbe0b0ab8ca0490");
superDerive::import(path : "c81fce53dede81ef89860aa3/4a701265a13057eef3bface7/b453944163f91ccf1477e3f0", version : "2b4306d8bcbe0b0ab8ca0490");


annotation { "Feature Type Name" : "Pipe Line", "Icon" : icon::BLOB_DATA}
export const pipeLine = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Pipe Line", "Filter" : EntityType.VERTEX || EntityType.EDGE}
        definition.pipeLine is Query;
        
        annotation { "Name" : "Extend Selection" }
        definition.extendSelection is boolean;
        
        annotation { "Name" : "Fitting Angle Tolerance" }
        isAngle(definition.angleTolerance, {(degree) : [0, 1, 180]} as AngleBoundSpec);

        annotation { "Name" : "Custom Parts" }
        definition.customParts is boolean;
        
        if (definition.customParts)
        {
            annotation { "Name" : "Fittings Studio" }
            definition.fittingStudio is PartStudioData;
            
            annotation { "Name" : "Fitting Axes to Enum type map variable name", "Default": "axesToFittingType" }
            definition.axesToFittingTypeKey is string;
            
            annotation { "Name" : "Pipes Studio", "Default": {buildFunction : pipeStudio::build, configuration: {}, partQuery: qEverything(EntityType.BODY)} as PartStudioData}
            definition.pipeStudio is PartStudioData;
        } else
        {
            annotation { "Name" : "Pipe OD", "Lookup Table" : shared::pipeSizeLookupTable }
            definition.od is LookupTablePath;
            
        }
        
        annotation { "Name" : "Instantiate Parts", "Default" : false }
        definition.instantiateParts is boolean;
        
        if (definition.instantiateParts)
        {
            superDerive::includeVariablesPredicate(definition);
        }
        
    }
    {
        const instantiatorLambda = function(id is Id, definition is map)
        {
            return modifiedInstantiator::newInstantiator(id, definition);
        };
        const addInstanceLambda = function(instantiator, partStudio, definition is map)
        {
            return modifiedInstantiator::addInstance(instantiator, partStudio, definition);
        };
        // Use default partStudios if not specified.
        println("definition.fittingStudio: " ~ definition.fittingStudio);
        println("definition.fittingStudio: " ~ canBeBuildFunction(definition.fittingStudio.buildFunction));
        definition = prepareStandardParts(context, id, definition);    
        println("definition.fittingStudio: " ~ definition.fittingStudio);
        
        debug(context, qTangentConnectedEdges(definition.pipeLine));
        
        const fittingsQ = qEntityFilter(definition.pipeLine, EntityType.VERTEX);
        const insertedFitting = insertFitting::insertFittingMateConnectors(context, id, {'fittingPoints':fittingsQ, allowCustomFittings: definition.allowCustomFittings, 'fittingStudio': definition.fittingStudio});
        if (definition.instantiateParts)
        {
            instantiateParts(context, id, {'fittingStudio' :definition.fittingStudio, 'insertedFitting' : insertedFitting, 'pipeStudio' : definition.pipeStudio, 'angleTolerance' :definition.angleTolerance, 'pipeLine' : definition.pipeLine,
            'instantiator': instantiatorLambda, 'addInstance': addInstanceLambda});
        }
    });

function prepareStandardParts(context is Context, id is Id, definition is map)
{   
    const odValue = getLookupTable(shared::pipeSizeLookupTable, definition.od);
    definition.fittingStudio = canBeBuildFunction(definition.fittingStudio.buildFunction)? definition.fittingStudio : {buildFunction: fittingStudio::build, configuration : {nominalOd: odValue}, partQuery: qEverything(EntityType.BODY)} as PartStudioData;
    definition.pipeStudio = canBeBuildFunction(definition.pipeStudio.buildFunction)? definition.pipeStudio : {buildFunction: pipeStudio::build, configuration : {nominalOd: odValue}, partQuery: qEverything(EntityType.BODY)} as PartStudioData;
    return definition;
}

function getPipeLine(context is Context, id is Id, definition is map)
{
    return getConnectedLinesAndPoints(context, id, definition.pipeLine);
}

function getConnectedLinesAndPoints(context is Context, id is Id, pipeLine is Query)
{
    var result = {};
    var newPoints = evaluateQuery(context, qEntityFilter(pipeLine, EntityType.VERTEX));
    var newLines = evaluateQuery(context, qEntityFilter(pipeLine, EntityType.EDGE));
    while(size(newPoints) > 0 || size(newLines) > 0)
    {
        for (var point in newPoints)
        {
        }
        for (var line in newLines)
        {
            
        }
    }
    return result;
}
    
function instantiateParts(context is Context, id is Id, definition is map)
{
    const insertedFitting = definition.insertedFitting;
    const fittingResult = mcInstantiator::buildTypedMateConnectors(context, id+"typedMCInstantiation1", {'fittingPartStudio':definition.fittingStudio, 'mateConnectors': insertedFitting.mateConnectors, 'pipeLine' : definition.pipeLine});
    var debugFittings =[];
    
    var mateConnectors = evaluateQuery(context, insertedFitting.mateConnectors);
    for (var i = 0; i < size(insertedFitting.confidences); i += 1)
    {
        if (acos(insertedFitting.confidences[i].maxDotProduct) > definition.angleTolerance)
        {
            debugFittings = append(debugFittings, shared::getAttributeType(context, mateConnectors[i], mcInstantiator::PART_INSTANTIATION_RESULT_KEY)['addedPart']);
        }
    }
    if (size(debugFittings) > 0)
    {
        setProperty(context, {
            "entities" : qUnion(debugFittings),
            "propertyType" : PropertyType.APPEARANCE,
            "value" : color(1, 0, 0)
        });
    }
    const pipeMateConnectors = qUnion(fittingResult.postInstantiationResults.resultantMateConnectors);
    mcInstantiator::buildTypedMateConnectors(context, id+"typedMCInstantiation2", {'fittingPartStudio':definition.pipeStudio, 'mateConnectors': pipeMateConnectors, 'subQuery': definition.pipeLine, 'attributes': fittingResult.postInstantiationResults.resultantAttributes});
    
    // Delete used mate connectors
    opDeleteBodies(context, id + "deleteBodies1", {
            "entities" : qBodyType(shared::getEntitiesWithAttributeType(context, qUnion([pipeMateConnectors, insertedFitting.mateConnectors]), mcInstantiator::PART_INSTANTIATION_RESULT_KEY), BodyType.MATE_CONNECTOR)
    });
}
