FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");

export function dimaeterOfCircle(area is ValueWithUnits)
{
    return sqrt(area*4/PI);
}

export function floorNearest(maxValue, sourceArray is array)
{
    const sortedArray = sort(sourceArray, function(a,b)
    {
        const diff = a-b;
        if (diff is ValueWithUnits)
        {
            return diff.value;
        }
        return diff;
    });
    var lastEntry = sortedArray[0];
    for (var entry in sortedArray)
    {
        if (entry > maxValue)
        {
            break;
        }
        lastEntry = entry;
    }
    return lastEntry;
}

// Linearly space an array, inclusive
export function linSpace(start, stop, num)
{
    const increment = (stop-start)/(num-1);
    var result = [];
    for (var i = 0; i < num; i += 1)
    {
        result = append(result, start + i*increment);
    }
    return result;
}

// Return an array composed of 0s
export function zeros(n)
{
    var result = [];
    for (var i = 0; i < n; i += 1)
    {
        result = append(result, 0);
    }
    return result;
}


// Finitely integrate a function across input parameters
export function integrate(func is function, a, b, iterations)
{
    const inputs = linSpace(a, b, iterations);
    const increment = (b-a)/iterations;
    var result = func(a) * increment;
    for (var i = 1; i < iterations; i += 1)
    {
        result = result + func(inputs[i]) * increment;
    }
    return result;
}

////////////////////////////////////////////////////  MATH TESTS //////////////////////////////////////////////////////////

annotation { "Feature Type Name" : "Math Test" }
export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const outsideVar = 5;
        const integrationResult = integrate(function(x)
        {
            return x^5+outsideVar;
        }, 0, 4, 500);
        println("Integration result: " ~ integrationResult);
        const myArray = [3*inch, 5*meter, 50*centimeter, 1.8*inch, 0.5*inch];
        println("floorNearest result: " ~ floorNearest(2*inch, myArray) ~ " And should be: " ~ 1.8*inch);
    });
    

