FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");
import(path : "ce024f31e19b8aabf789b4c8", version : "7af2751a7071d578bef7e9ca");


export enum SDR
{
    _13_5, _26, _64
}

export const SDR_TO_NUMBER = {
    (SDR._13_5) : 13.5,
    (SDR._26) : 26,
    (SDR._64) : 64
};

export enum ND
{
    _0_375,
    _0_5,
    _0_75,
    _1,
    _1_25,
    _1_5,
    _2,
    _2_5,
    _3,
    _4,
    _5,
    _6,
    _8,
    _10,
    _12
}

export const ND_TO_OD = {
    (ND._0_375) : 0.687 * inch,
    (ND._0_5) : 0.84 * inch,
    (ND._0_75) : 1.05 * inch,
    (ND._1) : 1.315 * inch,
    (ND._1_25) : 1.660 * inch,
    (ND._1_5) : 1.9 * inch,
    (ND._2) : 2.375 * inch,
    (ND._2_5) : 2.875 * inch,
    (ND._3) : 3.5 * inch,
    (ND._4) : 4.5 * inch,
    (ND._5) : 5.563 * inch,
    (ND._6) : 6.625 * inch,
    (ND._8) : 8.625 * inch,
    (ND._10) : 10.75 * inch,
    (ND._12) : 12.75 * inch
};

export const ND_OD_ENTRIES_SORTED_BY_OD = sortMapIntoArray(ND_TO_OD, function(a, b)
{
    return a.value - b.value;
});

export function nDFromIdAndSdr(insideDiameter is ValueWithUnits, sdr is SDR) returns ND
{
    for (var entry in ND_OD_ENTRIES_SORTED_BY_OD)
    {
        if (iDFromSdrAndOd(entry.value,sdr) > insideDiameter)
        {
            return entry.key;
        }
    }
}

export function iDFromSdrAndOd(od is ValueWithUnits, sdr is SDR) returns ValueWithUnits
{
    return od - 2*od/SDR_TO_NUMBER[sdr];
}