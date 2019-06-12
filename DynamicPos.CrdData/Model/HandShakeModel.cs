namespace DynamicPos.CrdData.Model
{

    /// <summary>
    /// HandShake Modeli
    /// </summary>
    public class HandShakeModel
    {

        #region Properties

        /// <summary>
        /// Bağlantı tipi :
        /// "RS232" : Com port bağlantısı
        /// "TCPIP" : Ip bağlantısı
        /// </summary>
        public string PosConnectionType { get; set; }

        /// <summary>
        /// PosConnectionType == TCPIP ise bu özellik pos un ip adresini tutar.
        /// </summary>
        public string PosIp { get; set; }


        /// <summary>
        /// PosConnectionType == TCPIP ise bu özellik pos un port numarasını tutar.
        /// </summary>
        public string PosIpPort { get; set; }

        /// <summary>
        /// SerialPort bağlantı noktasını tutar
        /// COMX
        /// </summary>
        public string ComPort { get; set; }

        /// <summary>
        /// Bağlantı şifreli mi olacak?
        /// 1 dışındaki tüm durumlarda şifreli olur.
        /// </summary>
        public string EncDisable { get; set; }
        /// <summary>
        /// Kasiyer Id numarası
        /// 00 01 02 03
        /// </summary>
        public string CashierId { get; set; }

        /// <summary>
        /// Kasiyer şifresi
        /// </summary>
        public string CashierPassword { get; set; }

        /// <summary>
        /// Ecr Mode
        /// 02 Satış Modu, 03 Admin Modu
        /// </summary>
        public string EcrMode { get; set; }

        /// <summary>
        /// Cihazın üreticisi
        /// </summary>
        public string DeviceBrand { get; set; }

        /// <summary>
        /// Cihaz modeli
        /// </summary>
        public string DeviceModel { get; set; }

        /// <summary>
        /// Cihaz seri numarası
        /// </summary>
        public string DeviceSerial { get; set; }

        /// <summary>
        /// Cihaz elektronik kayıt ünitesi seri numarası
        /// </summary>
        public string DeviceEcrSerial { get; set; }
        #endregion

    }
}
