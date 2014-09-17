using System;
using DirectShowLib;
using DirectShowLib.BDA;
using DVBT2Viewer.BDA.Helpers;
using DVBT2Viewer.BDA.Helpers.HelperTypes;
using DVBT2Viewer.BDA.Interfaces;

namespace DVBT2Viewer.BDA
{
    internal abstract class DVBT2BaseGraphCommands
    {
        /// <summary>
        /// Build DVB-T/T2 tune request
        /// </summary>
        /// <param name="aFrequency">Tune frequency</param>
        /// <param name="aBandwidth">Tune bandwidth</param>
        /// <returns>ITuneRequest implementation</returns>
        protected ITuneRequest GetTuneRequest(int aFrequency, int aBandwidth)
        {
            if (aFrequency == -1 || aBandwidth == -1)
                throw new Exception("Invalid tune");

            ITuneRequest baseTuneRequest;
            GetGraph().TuningSpace.CreateTuneRequest(out baseTuneRequest);
            var tuneRequest = (IDVBTuneRequest)baseTuneRequest;
            var locator = (IDVBTLocator)new DVBTLocator();
            var hr = locator.put_CarrierFrequency(aFrequency);
            DsError.ThrowExceptionForHR(hr);
            hr = locator.put_Bandwidth(aBandwidth);
            DsError.ThrowExceptionForHR(hr);
            hr = tuneRequest.put_Locator(locator);
            DsError.ThrowExceptionForHR(hr);

            return tuneRequest;
        }

        /// <summary>
        /// Set active PLP ID
        /// </summary>
        /// <param name="plp">PLP ID</param>
        protected void SetPlp(int plp)
        {
            var plpSupported = KsPropertyHelper.KSSupported(GetGraph().TunerPin, KSPropSets.KSPROPSETID_BdaDigitalDemodulator,
                (int)KSPROPERTY_BDA_DIGITAL_DEMODULATOR.KSPROPERTY_BDA_PLP_NUMBER);
            if (plpSupported.HasFlag(KSPropertySupport.Set))
                KsPropertyHelper.KSSetNode(GetGraph().TunerPin, KSPropSets.KSPROPSETID_BdaDigitalDemodulator,
                    (int)KSPROPERTY_BDA_DIGITAL_DEMODULATOR.KSPROPERTY_BDA_PLP_NUMBER, plp);
        }
        
        protected abstract IDVBT2BaseGraphProps GetGraph();
    }
}
