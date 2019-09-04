FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");
shared::import(path : "513f6e32bda69f3ba7d654fd", version : "0a7b9c2c95b93fa0ecfe36de");
inserters::import(path : "5c10d1b6310aa17232b17fd2", version : "9bb2ba9505f9cae75f9200d2");

annotation { "Feature Type Name" : "Typed Mate Connector Instantiator" }
export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Mate Connectors To Instantiate", "Filter" : BodyType.MATE_CONNECTOR }
        definition.mateConnectors is Query;
        
        annotation { "Name" : "Sub Query with which to bound the instantiation", "Filter": EntityType.EDGE}
        definition.subQuery is Query;
        
        annotation { "Name" : "Fitting PartStudio" }
        definition.fittingPartStudio is PartStudioData;
        
    }
    {
        buildTypedMateConnectors(context, id, {fittingPartStudio: definition.fittingPartStudio, mateConnectors: definition.mateConnectors, subQuery: definition.subQuery});
    });


/**
 * Take all the mateConnectors and instantiate the specified part with the specified configuration within the MC's attributes
 * @param definition {{
 *      @field fittingPartStudio :: The partStudio from which to derive the parts
 *      @field subQuery :: The subQuery denoting the selection to be passed to the inserter
 *      @field mateConnectors :: The query for the mate connectors to be constructed
 * }}
 **/
export function buildTypedMateConnectors(context is Context, id is Id, definition is map)
{
    const typedMateConnectorsQ = qAttributeFilter(qBodyType(definition.mateConnectors, BodyType.MATE_CONNECTOR), { 'type' : PART_INSTANTIATION_KEY });
    const typedMateConnectorsQs = evaluateQuery(context, typedMateConnectorsQ);
    println('typedMateConnectors: ' ~ typedMateConnectorsQs);
    var partInstantiationInfos = [];
    const instantiator = newInstantiator(id);
    var addedParts = [];
    var inserterTypes = [];
    
    // Insertion pass
    for (var mateConnectorQ in typedMateConnectorsQs)
    {
        const partInstantiationInfo = shared::getAttributeType(context, mateConnectorQ, PART_INSTANTIATION_KEY);
        const addedPart = inserters::insert(context, id, {'mateConnectors': [mateConnectorQ], 
                                                        'partInstantiationInfo': partInstantiationInfo,
                                                        'instantiator': instantiator,
                                                        'partStudio': definition.fittingPartStudio,
                                                        'subQuery': definition.subQuery});
        setAttribute(context, {
                "entities" : mateConnectorQ,
                "attribute" : {'type': PART_INSTANTIATION_RESULT_KEY, 'addedPart': addedPart}
        });
        addedParts = append(addedParts, addedPart);
        partInstantiationInfos = append(partInstantiationInfos, partInstantiationInfo);
        inserterTypes = append(inserterTypes, partInstantiationInfo['inserterType']);
    }
    if (size(instantiator[].names)>0)
    {
        instantiate(context, instantiator);
    }
    var postInstantiationResults = undefined;
    // Post insertion pass for part modification
    for (var inserterType in inserterTypes)
    {
        postInstantiationResults = inserters::postInstantiation(context, id, {'partInstantiationInfos': partInstantiationInfos,
            'mateConnectors': qUnion(typedMateConnectorsQs), 
            'inserterType': inserterType,
            'addedParts': addedParts,
            'subQuery': definition.subQuery});
    }
    return {'addedParts': addedParts, 'postInstantiationResults': postInstantiationResults};
}

export const PART_INSTANTIATION_KEY = 'partInstantiationInfo';
export const PART_INSTANTIATION_RESULT_KEY = 'partInstantiationResult';