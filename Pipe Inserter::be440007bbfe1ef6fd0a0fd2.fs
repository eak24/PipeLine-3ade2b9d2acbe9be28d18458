FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");
shared::import(path : "513f6e32bda69f3ba7d654fd", version : "0a7b9c2c95b93fa0ecfe36de");

annotation { "Feature Type Name" : "Insert Pipe" }
export const pipeInserter = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        pipeInserterPredicate(definition);
    }
    {
        insertPipe(context, id, definition);
    });
    
export predicate pipeInserterPredicate(definition is map)
{
    annotation { "Name" : "Start", "Filter" : BodyType.MATE_CONNECTOR, "MaxNumberOfPicks" : 1 }
    definition.start is Query;
    
    annotation { "Name" : "End", "Filter" : BodyType.MATE_CONNECTOR, "MaxNumberOfPicks" : 1 }
    definition.end is Query;
    
    annotation { "Name" : "Lines", "Filter" : EntityType.EDGE}
    definition.subQuery is Query;
    
    annotation { "Name" : "Pipe Library"}
    definition.library is PartStudioData;
    
    annotation { "Name" : "Instantiator"}
    isAnything(definition.instantiator);
}

    
function insertPipe(context is Context, id is Id, definition is map)
{
    pipeInserterPredicate(definition);
    const start = evMateConnector(context, { "mateConnector" : definition.start });
    const lineQ = shared::getAttributeType(context, definition.start, 'partInstantiationInfo')['lineQuery'];
    // If an end isn't specified, try to find it by looking in the global context
    if (!(definition.end is Query))
    {
        // Abort if the line isn't within the subQuery;
        if (size(evaluateQuery(context, qIntersection([lineQ, definition.subQuery])))==0)
        {
            return;
        }
        const lineQMCs = qAttributeQuery({'type':'partInstantiationInfo', inserterType: 'pipeInserter', 'lineQuery': lineQ});
        definition.end = qSubtraction(lineQMCs, definition.start);
        // If the end is still not found, look for the other end of the line
        if (size(evaluateQuery(context, definition.end)) == 0)
        {
            const lineEndsQ = qAdjacent(lineQ, AdjacencyType.VERTEX, EntityType.VERTEX);
            definition.end = qFarthestAlong(lineEndsQ, start.zAxis);
        }
    }
    const end = evVertexPoint(context, {
            "vertex" : definition.end
    });
    const direction = end -start.origin;
    const length = evDistance(context, {
        "side0" : definition.start,
        "side1" : definition.end
    }).distance;
    const transform = toWorld(coordSystem(start.origin, perpendicularVector(direction), direction));
    const usePassedInInstantiator = canBeInstantiator(definition.instantiator);
    const instantiator = (usePassedInInstantiator ? definition.instantiator : newInstantiator(id)) as Instantiator;
    var pipeLibrary = definition.library;
    var result = undefined;
    const lineName = @transientIdToString(lineQ.transientId);
    println("lineQ for generated: " ~ lineName);
    if (instantiator[].names[lineName]==undefined) {
        result = addInstance(instantiator, pipeLibrary, {
            'transform': transform,
            'configurationOverride': {'Length' : length},
            'configuration': {'Length' : length},
            'name' : lineName
        });
    }
    if (!usePassedInInstantiator)
    {
        instantiate(context, instantiator);
    }
    return result;
}



export const inserterTypeKey = 'pipeInserter';
export const insert = function(context is Context, id is Id, definition is map)
{
    insertPipe(context, id, {'start': definition.mateConnectors[0], 
    'instantiator': definition.instantiator, 'subQuery': definition.subQuery, 'library': definition.partStudio});
};
export const postInstantiation = function(context is Context, id is Id, definition is map)
{
};