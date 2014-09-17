using DirectShowLib;
using DVBT2Viewer.BDA.Helpers;
using DVBT2Viewer.BDA.Helpers.HelperTypes;

namespace DVBT2Viewer.BDA.Graph
{
    class TunerKspNodeManager
    {
        private readonly IKsPropertySet ksProperty;

        internal TunerKspNodeManager(IKsPropertySet tunerKsProperty)
        {
            ksProperty = tunerKsProperty;
        }

        public  void SetPlpId(int plp)
        {
            KsPropertyHelper.KSSetNode(ksProperty, KSPropSets.KSPROPSETID_BdaDigitalDemodulator,
                (int)KSPROPERTY_BDA_DIGITAL_DEMODULATOR.KSPROPERTY_BDA_PLP_NUMBER, plp);
        }

        public int GetPlpCount()
        {
            return (int)KsPropertyHelper.KSGetNode(ksProperty, KSPropSets.KSPROPSETID_BdaDigitalDemodulator,
                (int)KSPROPERTY_BDA_DIGITAL_DEMODULATOR.KSPROPERTY_BDA_PLP_NUMBER, typeof(int));
        }
    }
}
