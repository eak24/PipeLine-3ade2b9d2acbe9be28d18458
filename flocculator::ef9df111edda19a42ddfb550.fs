FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");
export import(path : "onshape/std/variable.fs", version : "1135.0");

pc::import(path : "c1f5bd76824416da18879419", version : "cfbc3b7b245fefdf4d3557ca");
import(path : "9092a7bca4fb3d6a0ea39170", version : "269b4fa33957ad7e42f8a506");
export import(path : "07df3dc395e84ce7297d5b27", version : "6f458454d1f9bec78d2655bf");
import(path : "ce024f31e19b8aabf789b4c8", version : "7af2751a7071d578bef7e9ca");
export import(path : "72b8225f8da139f77cf79750", version : "3910a37dfc0f4f98194bc007");

export const flocculatorDesignDefaults = mergeMaps(flowParameterDefaults, {
        entranceLength : 1.5 * meter,
        maxChannelWidth : 42.0 * inch,
        maxLength : 6.0 * meter,
        gt : 37000,
        hL : 40.0 * centimeter,
        endWaterDepth : 2.0 * meter,
        timeToDrain : 30.0 * minute,
        sdr : SDR._64,
        widthBaffleSheet : 42.0 * inch,
        flocOutletWidth : 42.0 * inch,
        wallThickness : 15.0 * centimeter,
        channelNParity : ChannelNParity.EVEN,
        hsMinRatio : 3.0,
        hsMaxRatio : 6.0,
        baffleK : 2.5,
        channelNMin : 2});

export enum ChannelNParity
{
    ODD, EVEN, ANY
}

annotation { "Feature Type Name" : "Flocculator Design", "Feature Name Template": "###name = #value", "UIHint" : "NO_PREVIEW_PROVIDED", "Editing Logic Function" : "variableEditLogic"}
export const flocculatorDesignFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        variableNamePredicate(definition);
        
        flowParametersPredicate(definition);
        
        annotation { "Group Name" : "Geometric Constraints", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Entrance Tank Length" }
            isLength(definition.entranceLength, {(meter) : [0, 1.5, 10]} as LengthBoundSpec);
            
            annotation { "Name" : "Maximum Channel Width" }
            isLength(definition.maxChannelWidth, {(inch) : [0, 42 ,100]} as LengthBoundSpec);
            
            annotation { "Name" : "Maximum Channel Length" }
            isLength(definition.maxLength, {(meter) : [0, 6 ,10]} as LengthBoundSpec);
            
            annotation { "Name" : "End Water Depth" }
            isLength(definition.endWaterDepth, {(meter) : [1,2,4]} as LengthBoundSpec);
            
            annotation { "Name" : "Collision Potential" }
            isReal(definition.gt, {(unitless) : [0,37000,200000]} as RealBoundSpec);
            
            annotation { "Name" : "Time To Drain", "Default" : "10*second" }
            isAnything(definition.timeToDrain);
            
            annotation { "Name" : "Pipe SDR" }
            definition.sdr is SDR;
            
            // definition.drainPipe = Pipe()
            
            annotation { "Name" : "Width of Baffle Sheet" }
            isLength(definition.widthBaffleSheet, {(inch) : [0, 42 ,100]} as LengthBoundSpec);
            
            annotation { "Name" : "Flocculator Outlet Width" }
            isLength(definition.flocOutletWidth, {(inch) : [0, 42 ,100]} as LengthBoundSpec);
            
            annotation { "Name" : "Wall Thickness" }
            isLength(definition.wallThickness, {(meter) : [0,0.15,0.5]} as LengthBoundSpec);
            
            annotation { "Name" : "Channel Parity Type" }
            definition.channelNParity is ChannelNParity;
        }
        
        annotation { "Group Name" : "Expert Inputs", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Minimum Allowed H/S Ratio" }
            isReal(definition.hsMinRatio, {(unitless) : [0,3,10]} as RealBoundSpec);
            
            annotation { "Name" : "Maximum Allowed H/S Ratio" }
            isReal(definition.hsMaxRatio, {(unitless) : [0,6,10]} as RealBoundSpec);
            
            annotation { "Name" : "Baffle K" }
            isReal(definition.baffleK, {(unitless) : [0,2.5,10]} as RealBoundSpec);
            
            annotation { "Name" : "Minimum Number of Channels" }
            isReal(definition.channelNMin, {(unitless) : [0,2,10]} as RealBoundSpec);
        }
        
    }
    {
        // Any definition verification needed
        definition.temp = (definition.temp+273.15)*kelvin;
        definition.newtons = definition.newtons*newton;
        
        // Call the design functions
        definition.viscosityKinematic = pc::viscosityKinematic(definition.temp);
        definition.velocityGradientAverage = velocityGradientAverage(definition);
        definition.retentionTime = retentionTime(definition);
        definition.volume = volume(definition);
        definition.channelWidthMinHSRatio = channelWidthMinHSRatio(definition);
        definition.channelWidthMin = channelWidthMin(definition);
        definition.lengthMaxVolume = lengthMaxVolume(definition);
        definition.channelLength = channelLength(definition);
        definition.channelsN = channelsN(definition);
        definition.channelWidthMinGt = channelWidthMinGt(definition);
        definition.channelWidth = channelWidth(definition);
        definition.expansionHMax = expansionHMax(definition);
        definition.expansionN = expansionN(definition);
        definition.expansionH = expansionH(definition);
        definition.baffleS = baffleS(definition);
        definition.obstacleN = obstacleN(definition);
        definition.contractionS = contractionS(definition);
        definition.obstaclePipeOd = obstaclePipeOd(definition);
        
        debugPrint(context, definition);
    
        assignVariable(context, id, {
                "variableType" : VariableType.ANY,
                "name" : definition.name,
                "anyValue" : definition
        });        
    }, flocculatorDesignDefaults
);

// The average velocity gradient of water.
function velocityGradientAverage(d is map)
{
    return ((gravity * d.hL) /
                (d.viscosityKinematic * d.gt));
}

// The hydraulic retention time neglecting the volume created by head loss.
function retentionTime(d is map)
{
    return d.gt / d.velocityGradientAverage;
}

function volume(d is map) 
{
    return d.q*d.retentionTime;
}

function channelWidthMinHSRatio(d is map)
{
    return (d.hsMinRatio*d.q/d.endWaterDepth) * (d.baffleK/(2*d.endWaterDepth*d.viscosityKinematic*d.velocityGradientAverage^2))^(1/3);
}

function channelWidthMin(d is map)
{
    return min(d.channelWidthMinHSRatio, d.widthBaffleSheet);
}

function channelsN(d is map)
{
    var proposed = d.channelNMin;
    if (d.q < 16 * liter / second)
    {
        proposed=1;
    }
    else
    {
        const channelN = (d.volume/(d.widthBaffleSheet*d.endWaterDepth) + d.entranceLength)/d.channelLength;
        if (d.channelNParity == ChannelNParity.EVEN)
        {
            proposed= ceil(channelN, 2);
        }
        else if (d.channelNParity == ChannelNParity.ODD)
        {
            proposed= ceil(channelN, 2)-1;
        }
        else if (d.channelNParity == ChannelNParity.ANY)
        {
            proposed= ceil(channelN, 1);
        }
    }
    return max(proposed, d.channelNMin);
}

// The channel width minimum regarding the collision potential.
function channelWidthMinGt(d is map)
{
    return d.volume/(d.endWaterDepth*(d.channelsN*d.channelLength-d.entranceLength));
}

function channelWidth(d is map)
{
    return ceil(max(d.channelWidthMinGt, d.channelWidthMin), centimeter);
}

// The maximum length depeneding on the volume.
function lengthMaxVolume(d is map)
{
    return d.volume/(d.channelNMin*d.channelWidthMin*d.endWaterDepth);
}

// The channel length.
function channelLength(d is map)
{
    return min(d.maxLength, d.lengthMaxVolume);
}

// The maximum distance between expansions for the largest allowable H/S ratio.
function expansionHMax(d is map)
{
    return (d.baffleK/(2*d.viscosityKinematic*d.velocityGradientAverage^2)*(d.q*d.hsMaxRatio/d.channelWidth)^3)^(1/4);
}

// The minimum number of expansions per baffle space.
function expansionN(d is map)
{
    return ceil(d.endWaterDepth/d.expansionHMax, 1);
}

// The height between flow expansions.
function expansionH(d is map)
{
    return d.endWaterDepth/d.expansionN;
}

// The spacing between baffles.
function baffleS(d is map)
{
    return (d.baffleK/(2*d.expansionH*d.velocityGradientAverage^2*d.viscosityKinematic))^(1/3)*d.q/d.channelWidth;
}

// The number of obstacles per baffle.
function obstacleN(d is map)
{
    return d.endWaterDepth/d.expansionH-1;
}

// The space in the baffle by which the flow contracts.
function contractionS(d is map)
{
    return d.baffleS*0.6;
}

// The outer diameter of an obstacle pipe. If the available pipe is greater than 1.5 inches, the obstacle offset will become false.
function obstaclePipeOd(d is map)
{
    return 2*inch;
}

