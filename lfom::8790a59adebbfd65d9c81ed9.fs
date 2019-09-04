FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");
export import(path : "onshape/std/variable.fs", version : "1135.0");

import(path : "9092a7bca4fb3d6a0ea39170", version : "269b4fa33957ad7e42f8a506");
import(path : "bee595512c86d688fcb12946", version : "aec52e4eeefe49a438fbbaf8");
import(path : "ef9a0209c0aa76f001749d5a", version : "354a414930020956ba6cf7e3");
import(path : "c1f5bd76824416da18879419", version : "cfbc3b7b245fefdf4d3557ca");
export import(path : "07df3dc395e84ce7297d5b27", version : "6f458454d1f9bec78d2655bf");
import(path : "ce024f31e19b8aabf789b4c8", version : "7af2751a7071d578bef7e9ca");
export import(path : "72b8225f8da139f77cf79750", version : "3910a37dfc0f4f98194bc007");

export const lfomDesignDefaults = mergeMaps(flowParameterDefaults, {
        drillBits : DRILL_BITS,
        hL : 40.0 * centimeter,
        orificeS : 0.5*centimeter,
        safetyFactor : 1.5,
        sdr : SDR._26});

annotation { "Feature Type Name" : "LFOM Design", "Feature Name Template": "###name = #value", "UIHint" : "NO_PREVIEW_PROVIDED", "Editing Logic Function" : "variableEditLogic"}
export const lfomDesignFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        variableNamePredicate(definition);
        
        flowParametersPredicate(definition);
        
        annotation { "Name" : "Space Left Between Orifices" }
        isLength(definition.orificeS, {(centimeter) : [0,0.5,4]} as LengthBoundSpec);
            
        annotation { "Name" : "Pipe SDR" }
        definition.sdr is SDR;
        
        annotation { "Group Name" : "Expert Inputs", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Safety Factor" }
            isReal(definition.safetyFactor, {(unitless) : [0,1.5,4]} as RealBoundSpec);
            
            annotation { "Name" : "Drill Bits", "Default" : "[0.25*inch, 0.5*inch, 1*inch]" }
            isAnything(definition.drillBits);
            
            annotation { "Name" : "Max Number of Rows" }
            isReal(definition.rowNMax, {(unitless) : [0,20,100]} as RealBoundSpec);
            
            annotation { "Name" : "Min Number of Rows" }
            isReal(definition.rowNMin, {(unitless) : [0,4,10]} as RealBoundSpec);
        }
        
    }
    {
        // Any definition verification needed
        definition.temp = (definition.temp+273.15)*kelvin;
        if (definition.drillBits == undefined)
        {
            definition.drillBits = DRILL_BITS;
        }
        
        
        // Call the design functions
        definition.viscosityKinematic = viscosityKinematic(definition.temp);
        definition.rowN = rowN(definition);
        definition.rowB = rowB(definition);
        definition.velocityCritical = velocityCritical(definition);
        definition.pipeAMin = pipeAMin(definition);
        definition.pipeNd = pipeNd(definition);
        definition.topRowOrificeA = topRowOrificeA(definition);
        definition.orificeDMax = orificeDMax(definition);
        definition.orificeD = orificeD(definition);
        definition.orificeA = orificeA(definition);
        definition.orificeNMaxPerRow = orificeNMaxPerRow(definition);
        definition.qPerRow = qPerRow(definition);
        definition.orificeHPerRow = orificeHPerRow(definition);
        definition.orificeNPerRow = orificeNPerRow(definition);
        
        debugPrint(context, definition);
    
        assignVariable(context, id, {
                "variableType" : VariableType.ANY,
                "name" : definition.name,
                "anyValue" : definition
        });        
    }, lfomDesignDefaults
);

// The width of a stout weir at a given elevation, as represented by h.
function stoutWPerFlow(d is map, h)
{
    return 2/((2*gravity*h)^0.5*VC_ORIFICE_RATIO*PI*d.hL);
}

// The number of rows.
function rowN(d is map)
{
    const nEstimated = d.hL*PI/(2*stoutWPerFlow(d, d.hL)*d.q);
    return min(d.rowNMax, max(d.rowNMin, round(nEstimated)));
}

// The distance center to center between each row of orifices.
function rowB(d is map) 
{
    return d.hL/d.rowN;
}

// The average vertical velocity of the water inside the LFOM pipe at the bottom of the orfices.
function velocityCritical(d is map)
{
    return (4 / (3*PI) * (2*gravity*d.hL)^(1/2));
}

// The minimum cross-sectional area of the LFOM pipe assuring a safety factor.
function pipeAMin(d is map)
{
    return d.safetyFactor*d.q/d.velocityCritical;
}

// The nominal diameter of the LFOM pipe.
function pipeNd(d is map)
{
    const insideDiameter = dimaeterOfCircle(d.pipeAMin);
    return nDFromIdAndSdr(insideDiameter, d.sdr);
}

// The orifice area corresponding to the top row of orifices.
function topRowOrificeA(d is map)
{
    const z = d.hL - 0.5 * d.rowB;
    return stoutWPerFlow(d, z) * d.q * d.rowB;
}

// The maximum orifice diameter.
function orificeDMax(d is map)
{
    return dimaeterOfCircle(d.topRowOrificeA);
}

// The actual orifice diameter.
function orificeD(d is map)
{
    const maxDrill = min(d.rowB, d.orificeDMax);
    return floorNearest(maxDrill, d.drillBits);
}

// The area of the actual drill bit.
function orificeA(d is map)
{
    return PI*d.orificeD^2/4;
}

// The max number of orifices allowed in each row.
function orificeNMaxPerRow(d is map)
{
    const c = PI * iDFromSdrAndOd(ND_TO_OD[d.pipeNd], d.sdr);
    const b = d.orificeD + d.orificeS;
    return floor(c/b);
}

// An array of flow at each row.
function qPerRow(d is map)
{
    return linSpace(d.q/d.rowN, d.q, d.rowN);
}

// The height of the center of each row of orifices.
function orificeHPerRow(d is map)
{
    return linSpace(d.orificeD/2, d.hL-d.orificeD/2, d.rowN);
}

// The flow rate through some number of submerged rows. Helper function.
function qSubmerged(d is map, submergedRowsN, orificeNPerRow)
{
    var flow = 0 * liter/second;
    for (var i = 0; i < size(orificeNPerRow); i += 1)
    {
        flow = flow + orificeNPerRow[i] * flowOrificeVertical(d.orificeD, d.rowB*(submergedRowsN+1)-d.orificeHPerRow[i], VC_ORIFICE_RATIO);
    }
    return flow;
}

// The number of orifices at each level.
function orificeNPerRow(d is map)
{
    const h = d.rowB - 0.5*d.orificeD;
    const flowPerOrifice = flowOrificeVertical(d.orificeD, h, VC_ORIFICE_RATIO);
    var nPerRow = [];
    for (var i = 0; i < d.rowN; i += 1)
    {
        const flowNeeded = d.qPerRow[i] - qSubmerged(d, i, nPerRow);
        const nOrificesReal = flowNeeded / flowPerOrifice;
        nPerRow = append(nPerRow, min(max(0, round(nOrificesReal)), d.orificeNMaxPerRow));
    }
    return nPerRow;
}

// The outer diameter of an obstacle pipe. If the available pipe is greater than 1.5 inches, the obstacle offset will become false.
function obstaclePipeOd(d is map)
{
    return 2*inch;
}

