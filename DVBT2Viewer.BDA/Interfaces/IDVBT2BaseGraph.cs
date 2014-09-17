using DirectShowLib;
using DirectShowLib.BDA;

namespace DVBT2Viewer.BDA.Interfaces
{
    #region DVB-T/T2 graph public properties
    internal interface IDVBT2BaseGraphProps
    {
        /// <summary>
        /// DVB-T/T2 tuning space
        /// </summary>
        ITuningSpace TuningSpace { get; }

        /// <summary>
        /// IKsPropertySet interface of Tuner output pin
        /// </summary>
        IKsPropertySet TunerPin { get; }
    }
    #endregion
}
