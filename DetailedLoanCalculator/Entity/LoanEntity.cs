using System;
using System.Collections.Generic;

namespace Albaraka.Utils.Calculator.LoanCalculator.Entity
{
    /// <summary>
    /// Kredi Entitysi
    /// </summary>
    [Serializable]
    public class LoanEntity
    {
        /// <summary>
        /// Kredi Türü;
        /// B: Bireysel,
        /// K: Kurumsal,
        /// L: Leasing
        /// </summary>
        public string CType { get; set; }
        /// <summary>
        /// Kredi Anapara Tutarı
        /// </summary>
        public double CPrincipalAmount { get; set; }
        /// <summary>
        /// Kredi Kar Tutarı
        /// </summary>
        public double CProfitAmount { get; set; }
        /// <summary>
        /// Kredi Bsmv Tutarı
        /// </summary>
        public double CBsmvAmount { get; set; }
        /// <summary>
        /// Kredi Kkdf Tutarı
        /// </summary>
        public double CKkdfAmount { get; set; }
        /// <summary>
        /// Kredi Kdv Tutarı
        /// </summary>
        public double CKdvAmount { get; set; }
        /// <summary>
        /// Kredi Taksit Sayısı
        /// </summary>
        public short CInstallmentCount { get; set; }
        /// <summary>
        /// Kredi Ödeme Periyodu;
        /// 1: ayda bir,
        /// 2: 2 ayda bir,
        /// 3: 3 ayda bir,
        /// 4: 4 ayda bir,
        /// ...
        /// </summary>
        public byte CInstallmentPeriod { get; set; }
        /// <summary>
        /// Kredi Kar Oranı
        /// </summary>
        public double CProfitRate { get; set; }
        /// <summary>
        /// Kredi Uygulanan Kar Oranı(sadece Katkı Paylı Kredi için CProfitRate'ten farklı olur)
        /// </summary>
        public double CAppliedProfitRate { get; set; }
        /// <summary>
        /// Kredi Bsmv Oranı
        /// </summary>
        public double CBsmvRate { get; set; }
        /// <summary>
        /// Kredi Kkdf Oranı
        /// </summary>
        public double CKkdfRate { get; set; }
        /// <summary>
        /// Kredi Kdv Oranı
        /// </summary>
        public double CKdvRate { get; set; }
        /// <summary>
        /// Kredi Kullandırım Tarihi
        /// </summary>
        public DateTime CValueDate { get; set; }
        /// <summary>
        /// İlk Taksit Tarihi
        /// </summary>
        public DateTime CFirstInstallmentDate { get; set; }
        /// <summary>
        /// Kredi Katkı Payı Tutarı
        /// </summary>
        public double CIncentiveAmount { get; set; }
        /// <summary>
        /// Kredi Katkı Payı Oranı
        /// </summary>
        public double CIncentiveRate { get; set; }
        /// <summary>
        /// Kredinin Uygulanan Kar oranının çekileceği Oran. Bu oran için Katkı Payı Tutarı ve Oranı Hesaplanır
        /// </summary>
        public double? CIncentiveAppliedProfitRate { get; set; }
        /// <summary>
        /// Kredi Taksit Listesi
        /// </summary>
        public List<AmortizationScheduleRow> AmortizationSchedule { get; set; } = new List<AmortizationScheduleRow>();
    }

    /// <summary>
    /// Amortization schedule row.
    /// Taksit Entitysi
    /// </summary>
    public class AmortizationScheduleRow
    {
        /// <summary>
        /// Taksit Numarası
        /// </summary>
        public int IRowNumber { get; set; }
        /// <summary>
        /// Taksit Periyod Başlangıç Tarihi
        /// </summary>
        public DateTime IValueDate { get; set; }
        /// <summary>
        /// Taksit Tarihi/Taksit Periyod Bitiş Tarihi
        /// </summary>
        public DateTime IMaturityDate { get; set; }
        /// <summary>
        /// Taksit Anapara Tutarı
        /// </summary>
        public double IPrincipalAmount { get; set; }
        /// <summary>
        /// Taksit Kar Tutarı
        /// </summary>
        public double IProfitAmount { get; set; }
        /// <summary>
        /// Taksit Bsmv Tutarı
        /// </summary>
        public double IBsmvAmount { get; set; }
        /// <summary>
        /// Taksit Kkdf Tutarı
        /// </summary>
        public double IKkdfAmount { get; set; }
        /// <summary>
        /// Taksit Kdv Tutarı
        /// </summary>
        public double IKdvAmount { get; set; }
        /// <summary>
        /// Taksit Kar Oranı
        /// </summary>
        public double IProfitRate { get; set; }
        /// <summary>
        /// Taksit Bsmv Oranı
        /// </summary>
        public double IBsmvRate { get; set; }
        /// <summary>
        /// Taksit Kkdf Oranı (kind of TAX)
        /// </summary>
        public double IKkdfRate { get; set; }
        /// <summary>
        /// Taksit Kdv Oranı (kind of TAX)
        /// </summary>
        public double IKdvRate { get; set; }
        /// <summary>
        /// Taksit Tutarı (kind of TAX)
        /// </summary>
        public double IAmount { get; set; }
        /// <summary>
        /// Taksit Kalan Anapara Tutarı(Taksitten Önceki)
        /// </summary>
        public double IPrincipalBalance { get; set; }
        /// <summary>
        /// Taksitten Devreden Kar Tutarı
        /// </summary>
        public double IDeferredProfit { get; set; }
        /// <summary>
        /// Taksit Sabitleme Seçenekleri
        /// </summary>
        public FixityTypes IFixity { get; set; }
    }

    /// <summary>
    /// Fixity types.
    /// Taksit Sabitliği Bayrakları
    /// </summary>
    [Flags]
    public enum FixityTypes
    {
        /// <summary>
        /// Sabitlik Yok
        /// </summary>
        None = 0,
        /// <summary>
        /// Taksit Tutarı Değişmeyecek 
        /// </summary>
        InstallmentAmount = 1,
        /// <summary>
        /// Kar Oranı Değişmeyecek
        /// </summary>
        ProfitRate = 2,
        /// <summary>
        /// BSMV Oranı Değişmeyecek
        /// </summary>
        BsmvRate = 4,
        /// <summary>
        /// KKDF Oranı Değişmeyecek
        /// </summary>
        KkdfRate = 8,
        /// <summary>
        /// KDV Oranı Değişmeyecek
        /// </summary>
        KdvRate = 16,
        /// <summary>
        /// Kitlenmiş Taksit - Hiç bir değer değişmeyecek
        /// </summary>
        All = InstallmentAmount | ProfitRate | BsmvRate | KkdfRate | KdvRate
    }
}