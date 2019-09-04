FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");

// Only accepts maps with a max key length of 30 characters
export function printMap(definition is map)
{
    const arrayed = sortMapIntoArray(definition, function(a,b)
    {
        return letterToValue(a.key) - letterToValue(b.key);
    });
    const bufferSpaces = 4;
    const maxKeyLength = 30;
    var i=0;
    for (var entry in arrayed)
    {
        println(entry.key ~ makeSpace(4) ~ makeSpace(maxKeyLength-size(splitIntoCharacters(entry.key))) ~ toString(entry.value));
        i = i+1;
    }
}

// sort function takes in two map entries. Each entry looks like: {key: "myKey", value: "myValue"}
export function sortMapIntoArray(mapToSort, sortFunction is function)
{
    var arrayed = [];
    for (var entry in mapToSort)
    {
        arrayed = append(arrayed, entry);
    }
    arrayed = sort(arrayed, function(a,b)
    {
        return sortFunction(a,b);
    });
    return arrayed;
}

function makeSpace(nSpaces)
{
    var result = '';
    for (var i = 0; i < nSpaces; i += 1)
    {
        result = result ~ ' ';
    }
    return result;
}

function letterToValue(letter is string)
{
    const letterToValueMap= {
        "[A-a]" : 1,
        "[B-b]" : 2,
        "[C-c]" : 3,
        "[D-d]" : 4,
        "[E-e]" : 5,
        "[F-f]" : 6,
        "[G-g]" : 7,
        "[H-h]" : 8,
        "[I-i]" : 9,
        "[J-j]" : 10,
        "[K-k]" : 11,
        "[L-l]" : 12,
        "[M-m]" : 13,
        "[N-n]" : 14,
        "[O-o]" : 15,
        "[P-p]" : 16,
        "[Q-q]" : 17,
        "[R-r]" : 18,
        "[S-s]" : 19,
        "[T-t]" : 20,
        "[U-u]" : 21,
        "[V-v]" : 22,
        "[W-w]" : 23,
        "[X-x]" : 24,
        "[Y-y]" : 25,
        "[Z-z]" : 26
    };
    letter = splitIntoCharacters(letter)[0];
    for (var entry in letterToValueMap)
    {
        var regEx = entry.key;
        var value = entry.value;
        
        if (match(letter, regEx).hasMatch)
        {
            return value;
        }
        
    }
    return 0;
}

export function debugPrint(context is Context, mapToPrint is map)
{
    var debug;
    try
    {
        debug = getVariable(context, "debug");
    }
    
    if (debug != undefined && debug == true)
    {
        printMap(mapToPrint);
    }
}
