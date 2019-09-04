FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");
import(path : "ef9a0209c0aa76f001749d5a", version : "354a414930020956ba6cf7e3");
export import(path : "07df3dc395e84ce7297d5b27", version : "6f458454d1f9bec78d2655bf");

// Return the kinematic viscosity of water at a given temperature. Input in kelvin.
export function viscosityKinematic(temp is ValueWithUnits)
{
    return (viscosityDynamic(temp).value
            / densityWater(temp).value)*meter^2/second;
}

// Return the vertical flow rate of the orifice. Height is measured from the center of the orifice. So a height of 0 includes half the orifice.
export function flowOrificeVertical(diameter, height, rationVcOrifice)
{
    if (height > -diameter/2)
    {
        const flowVertical = integrate(function(z)
        {
            return diameter*sin(acos(z/(diameter/2)))*sqrt(height/meter-z/meter)*meter;
        }, -diameter/2, min(height, diameter/2), 100);
        return flowVertical * rationVcOrifice * sqrt(2*gravity.value)/second;
    }
    else
    {
        return 0;
    }
}

// Return the dynamic viscosity of water at a given temperature. Input in kelvin.
function viscosityDynamic(temp is ValueWithUnits)
{
    return (2.414 * (10^-5) * 10^(247.8 / (temp.value-(140))))*kilogram/(meter*second);
}

function densityWater(temp is ValueWithUnits)
{
    // TODO: change to a cubic spline interpolation from the table
    return 1000*kilogram/meter^3;
}
    