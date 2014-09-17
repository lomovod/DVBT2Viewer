using System;

namespace DVBT2Viewer.BDA.Helpers.HelperTypes
{
    class KSPropSets
    {
        /// <summary>
        /// KSPROPSETID_BdaDigitalDemodulator GUID defined in bdamedia.h
        /// </summary>
        public static readonly Guid KSPROPSETID_BdaDigitalDemodulator = new Guid("EF30F379-985B-4d10-B640-A79D5E04E1E0");
    }

    /// <summary>
    /// KSPROPERTY_BDA_DIGITAL_DEMODULATOR structure defined in bdamedia.h
    /// </summary>
    enum KSPROPERTY_BDA_DIGITAL_DEMODULATOR
    {
        KSPROPERTY_BDA_MODULATION_TYPE = 0,
        KSPROPERTY_BDA_INNER_FEC_TYPE,
        KSPROPERTY_BDA_INNER_FEC_RATE,
        KSPROPERTY_BDA_OUTER_FEC_TYPE,
        KSPROPERTY_BDA_OUTER_FEC_RATE,
        KSPROPERTY_BDA_SYMBOL_RATE,
        KSPROPERTY_BDA_SPECTRAL_INVERSION,
        KSPROPERTY_BDA_GUARD_INTERVAL,
        KSPROPERTY_BDA_TRANSMISSION_MODE,
        KSPROPERTY_BDA_ROLL_OFF,
        KSPROPERTY_BDA_PILOT,
        KSPROPERTY_BDA_SIGNALTIMEOUTS,
        KSPROPERTY_BDA_PLP_NUMBER
    }
}
