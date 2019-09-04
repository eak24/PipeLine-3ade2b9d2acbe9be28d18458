FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");
simpleInserter::import(path : "8f763e497f553339e9c7a3a9", version : "70ec2166c3d784ecd96f65b0");

export const inserterTypeKey = 'fittingInserter';
export const insert = function(context is Context, id is Id, definition is map) returns Query
{
    return simpleInserter::insert(context, id, definition);
    
};

// Hunt for resultant partInstantiationInfos that are connected through the lineQueries and construct a new partInstantiationInfo for the pipe.
export const postInstantiation = function(context is Context, id is Id, definition is map)
{
    const lineQueryByMateConnectors = makeLineQueryToMateConnectorsDictionary(context,id,{'partInstantiationInfos': definition.partInstantiationInfos,
                                                                                            'mateConnectors' : definition.mateConnectors,
                                                                                            'addedParts' : definition.addedParts});
    var resultantMateConnectors = [];
    var resultantAttributes = [];
    for (var entry in lineQueryByMateConnectors)
    {
        var lineQuery = entry.key;
        var mateConnectors = entry.value;
        const attributeToAdd = {'type' : 'partInstantiationInfo', 'inserterType' : 'pipeInserter', 'lineQuery' : lineQuery};
        resultantMateConnectors = append(resultantMateConnectors, qUnion(mateConnectors));
        resultantAttributes = append(resultantAttributes, attributeToAdd);
        setAttribute(context, {
                "entities" : qUnion(mateConnectors),
                "attribute" : attributeToAdd
        });
    }
    return {'resultantMateConnectors': resultantMateConnectors, 'resultantAttributes': resultantAttributes};
};

function makeLineQueryToMateConnectorsDictionary(context is Context, id is Id, definition is map)
{
    var result = {};
    const addedParts = definition.addedParts;
    const partInstantiationInfos = definition.partInstantiationInfos;
    for (var i = 0; i < size(definition.addedParts); i += 1)
    {
        const partInstantiationInfo = partInstantiationInfos[i];
        const addedPart = addedParts[i];
        const childMateConnectors = qBodyType(qOwnedByBody(addedPart, EntityType.VERTEX), BodyType.MATE_CONNECTOR);
        for (var j = 0; j < size(partInstantiationInfo.lineQueries); j += 1)
        {
            const edge = partInstantiationInfo.lineQueries[j];
            var childMateConnector = qAttributeFilter(childMateConnectors, {'type': 'fittingAxisMcId', 'id': j});
            if (result[edge] == undefined) 
            {
                result[edge] = [childMateConnector];
            }
            else
            {
                result[edge] = append(result[edge], childMateConnector);
            }
        }
    }
    return result;
}

