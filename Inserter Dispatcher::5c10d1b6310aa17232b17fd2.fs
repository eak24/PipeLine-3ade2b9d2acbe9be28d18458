FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");
fittingInserter::import(path : "441cc8ee91ab069b260e982d", version : "9094afde5f23b0e51137ba4a");
pipeInserter::import(path : "be440007bbfe1ef6fd0a0fd2", version : "2b9474f515933ff90a40b6ec");
simpleInserter::import(path : "8f763e497f553339e9c7a3a9", version : "70ec2166c3d784ecd96f65b0");

/**
 * An Inserter's job is to insert parts based solely on some partInstantiationInfo object AND a list of mateConnectors.
 * @param definition {{
 *      @field partInstantiationInfo: The information being used to insert the part
 *      @field mateConnector: The mate connector consumed in the construction of this part
 **/
export function insert(context is Context, id is Id, definition is map)
{
    const inserterType = definition.partInstantiationInfo.inserterType;
    return inserterFunctionByInserterType[inserterType]['insert'](context, id, definition);
}

export function postInstantiation(context is Context, id is Id, definition is map)
{
    const inserterType = definition.inserterType;
    return inserterFunctionByInserterType[inserterType]['postInstantiation'](context, id, definition);
}

const inserterFunctionByInserterType = {simpleInserter::inserterTypeKey: 
    {'insert':simpleInserter::insert, 'postInstantiation': simpleInserter::postInstantiation},
                                        fittingInserter::inserterTypeKey: 
    {'insert':fittingInserter::insert, 'postInstantiation': fittingInserter::postInstantiation},
                                        pipeInserter::inserterTypeKey: 
    {'insert':pipeInserter::insert, 'postInstantiation': pipeInserter::postInstantiation}};

