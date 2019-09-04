FeatureScript 1120;
import(path : "onshape/std/geometry.fs", version : "1120.0");

export const liter = 0.001*meter^3;
export const VOLUME_BOUNDS =
 {
      (liter) : [-500, 0.0025, 500]
 } as VolumeBoundSpec;
 
 export const FLOW_BOUNDS =
 {
      (liter/second) : [-500, 0.0025, 500]
 };

export const gravity = 9.8 * meter/second^2;
export const newton = kilogram * meter/second^2;
export const minute = 60 * second;

annotation { "Name" : "Kelvin", "Abbreviation" : "k" }
export const kelvin = { "value" : 1, "unit" : TEMPERATURE_UNITS } as ValueWithUnits;

export const NU = 1.002 * newton*second/meter^2;
export const M_PA = 1000 * newton/meter^2;
export const PVC_ROUGHNESS = 0.0015 * millimeter;

/////////////////////////////////// UNIT PREDICATES
export predicate isTemp(val)
{
    val is ValueWithUnits;
    val.unit == TEMPERATURE_UNITS;
}

export type LengthType typecheck isLength;

/////////////////////////////////// UNIT SPECS
export const FLOW_UNIT_SPEC = {"meter":3, "second" : -1} as UnitSpec;
export const VOLUME_UNIT_SPEC = {"meter":3} as UnitSpec;
export const KINEMATIC_VISCOSITY_UNIT_SPEC = {"meter":2, "second" : -1} as UnitSpec;
export const FORCE_UNIT_SPEC = {"meter":1, "second" : -2, "kilogram":1} as UnitSpec;


/**
 * Used to check the dimensions of an input that has to go in as an isAnything in the formal precondition.
 */
export function dimensionChecker(value is ValueWithUnits, unitSpec is UnitSpec, parameterId is string, boundSpec is map)
{
    if (boundSpec != undefined && !canBeBoundSpec(boundSpec))
    {
        throw regenError("Invalid BoundSpec: " ~ boundSpec);
    }
    if (!(value.unit == unitSpec)) 
    {
        const message = "Invalid units! Should be: " ~ unitSpec ~ ", but is: " ~ value.unit;
        if (parameterId != undefined)
        {
            throw regenError(message, [parameterId]);
        }
        else
        {
            throw regenError(message);
        }
    }
    for (var entry in boundSpec)
    {
        if (entry.value is array)
        {
            if (entry.value[0] * entry.key <= value && value <= entry.value[2] * entry.key)
                return true;
            throw regenError(ErrorStringEnum.PARAMETER_OUT_OF_RANGE);
        }
    }
}


// Volume Predicate

function verifyBounds(value, boundSpec is map) returns boolean
{
    for (var entry in boundSpec)
    {
        if (entry.value is array)
        {
            if (entry.value[0] * entry.key <= value && value <= entry.value[2] * entry.key)
                return true;
            throw regenError(ErrorStringEnum.PARAMETER_OUT_OF_RANGE);
        }
    }
    // Triggers error if boundSpec is invalid
}

/**
 * True for any value with volume units.
 */
predicate isVolume(val)
{
    val is ValueWithUnits;
    val.unit == VOLUME_UNITS;
}

export predicate isVolume(value, boundSpec is VolumeBoundSpec)
{
    isVolume(value);
    verifyBounds(value, boundSpec);
}

export type VolumeBoundSpec typecheck canBeVolumeBoundSpec;

/** Typecheck for VolumeBoundSpec */
export predicate canBeVolumeBoundSpec(value)
{
    canBeBoundSpec(value);
    for (var entry in value)
        isVolume(entry.key);
}