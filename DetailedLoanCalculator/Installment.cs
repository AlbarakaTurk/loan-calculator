using System;
using System.Collections.Generic;

using Albaraka.Utils.Calculator.LoanCalculator.Entity;

namespace Albaraka.Utils.Calculator.LoanCalculator
{
    /// <summary>
    /// Installment.
    /// </summary>
    public class Installment
    {
        //For "is equal to ZERO" comparison
        private const double EPSILON = .00001;

        public Installment()
        {
        }

        /// <summary>
        /// Calculates the installments.
        /// Taksit Listesi Oluştur
        /// </summary>
        /// <returns>The installments.</returns>
        /// <param name="loanEntity">Loan entity.</param>
        public LoanEntity CalculateInstallments(LoanEntity loanEntity)
        {
            //Katkı Paylı Hesaplama
            if (loanEntity.CIncentiveAmount > 0 || loanEntity.CIncentiveRate > 0 || loanEntity.CIncentiveAppliedProfitRate != null)
            {
                loanEntity = CalculateInstallmentsWithIncentive(loanEntity);
            }
            //Katkı Paysız Normal Hesaplama
            else
            {
                ////////////////////////////////////////////////////////
                loanEntity = PrepareScheduleTable(loanEntity);
                ////////////////////////////////////////////////////////
                loanEntity.CAppliedProfitRate = loanEntity.CProfitRate;
            }

            return loanEntity;
        }

        /// <summary>
        /// Calculates the installments with incentive.
        /// Taksit Listesi Oluştur (Katkı Paylı)
        /// </summary>
        /// <returns>The installments with incentive.</returns>
        /// <param name="loanEntity">Loan entity.</param>
        private LoanEntity CalculateInstallmentsWithIncentive(LoanEntity loanEntity)
        {
            if (loanEntity.CIncentiveAmount > 0 && (loanEntity.CIncentiveRate > 0 || loanEntity.CIncentiveAppliedProfitRate != null) ||
                loanEntity.CIncentiveRate > 0 && (loanEntity.CIncentiveAmount > 0 || loanEntity.CIncentiveAppliedProfitRate != null) ||
                loanEntity.CIncentiveAppliedProfitRate != null && (loanEntity.CIncentiveRate > 0 || loanEntity.CIncentiveAmount > 0))
            {
                throw new ArgumentException("Katkı Payı Hesabı için 3 Parametreden sadece biri dolu gönderilmeli", "CIncentiveAmount,CIncentiveRate,CIncentiveAppliedProfitRate");
            }

            //Katkı Payı tutarı veya Oranı Girildiyse Hesaplama
            if (loanEntity.CIncentiveAmount > 0 || loanEntity.CIncentiveRate > 0)
            {
                ////////////////////////////////////////////////////////
                loanEntity = PrepareScheduleTable(loanEntity);
                ////////////////////////////////////////////////////////

                if (loanEntity.CIncentiveRate > 0)
                {
                    loanEntity.CIncentiveAmount = loanEntity.CPrincipalAmount * (loanEntity.CIncentiveRate / 100.0d);
                }

                var incentiveCreditAmount = loanEntity.CIncentiveAmount / (1 + loanEntity.CBsmvRate + loanEntity.CKkdfRate + loanEntity.CKdvRate);
                var incentiveProfitAmount = (loanEntity.CProfitAmount / loanEntity.CPrincipalAmount) * incentiveCreditAmount;
                var calculatedTotalProfit = loanEntity.CProfitAmount - (incentiveCreditAmount + incentiveProfitAmount);

                //-0.01 ve 0.01 arasını 0 kabul ediyoruz
                //-0.01'den küçükse max değeri al
                if (calculatedTotalProfit < -0.01)
                {
                    //TODO max katkı payı oranını ve tutarını hesapla
                }
                //-0.01 ve 0.01 arası ise 0 olarak al
                else if (calculatedTotalProfit < 0.01)
                {
                    calculatedTotalProfit = 0;
                }

                //Tahmini başlangıç kar oranı alalım
                var newRate = loanEntity.CProfitRate - ((loanEntity.CProfitAmount - calculatedTotalProfit) / loanEntity.CProfitAmount);
                var biggestSmallRate = 0d;
                var smallestBigRate = 0d;

                //Kontrol için 1 ile 100 arası bir eşik değeri belirleyelim
                int treshholdValue = (int)Math.Sqrt(loanEntity.CPrincipalAmount / 600.0d);
                treshholdValue = treshholdValue > 100 ? 100 : (treshholdValue < 1 ? 1 : treshholdValue);

                // log amaçlı
                var rates = new List<double>();

                //calculatedTotalProfit ile hesaplanan toplam taksit kar tutarı tutana kadar oranı değiştir pymentPlan hesapla.
                while (Math.Abs(loanEntity.CProfitAmount - calculatedTotalProfit) >= treshholdValue)
                {
                    if (Math.Abs(biggestSmallRate) <= EPSILON && Math.Abs(smallestBigRate) <= EPSILON)
                    {
                        // ilk değer kalsın -> newRate = newRate 
                        newRate = RoundAway(newRate, 10);
                    }
                    else if (biggestSmallRate > 0 && Math.Abs(smallestBigRate) <= EPSILON)
                    {
                        // oransal yakınsama
                        newRate = RoundAway(newRate + (Math.Abs(calculatedTotalProfit - loanEntity.CProfitAmount) * newRate / loanEntity.CProfitAmount) * 1.1d, 10);
                    }
                    else if (Math.Abs(biggestSmallRate) <= EPSILON && smallestBigRate > 0)
                    {
                        // oransal yakınsama
                        newRate = RoundAway(newRate - (Math.Abs(calculatedTotalProfit - loanEntity.CProfitAmount) * newRate / loanEntity.CProfitAmount) * 1.1d, 10);
                    }
                    else
                    {
                        // oransal yakınsama
                        newRate = RoundAway((biggestSmallRate + smallestBigRate) / 2.0d, 10);
                    }

                    // log amaçlı
                    rates.Add(newRate);

                    //payment plan hesapla
                    ////////////////////////////////////////////////////////
                    loanEntity.CProfitRate = newRate;
                    loanEntity = PrepareScheduleTable(loanEntity);
                    ////////////////////////////////////////////////////////

                    // Yeni kar oranını hesapla
                    if (loanEntity.CProfitAmount > calculatedTotalProfit)
                    {
                        smallestBigRate = newRate;
                    }
                    else
                    {
                        biggestSmallRate = newRate;
                    }
                }

                loanEntity.CAppliedProfitRate = loanEntity.CProfitRate;

                //bireysel kredide - hesaplanan oran %0.01 altına düştüğünde oranı sıfırla
                if (loanEntity.CType == "B" && loanEntity.CProfitRate < 0.01d)
                {
                    ////////////////////////////////////////////////////////
                    loanEntity.CProfitRate = 0d;
                    loanEntity = PrepareScheduleTable(loanEntity);
                    ////////////////////////////////////////////////////////
                    loanEntity.CAppliedProfitRate = 0d;
                }
            }
            //Hangi Katkı Payı tutarı ile istenen Orana Çekileceğini belirle
            else if (loanEntity.CIncentiveAppliedProfitRate != null)
            {
                //Orjinal Kar oranıyla önce bi hesapla
                ////////////////////////////////////////////////////////
                loanEntity = PrepareScheduleTable(loanEntity);
                ////////////////////////////////////////////////////////

                var orjCProfitAmount = loanEntity.CProfitAmount;
                var orjCPrincipalAmount = loanEntity.CPrincipalAmount;
                var orjCBsmvRate = loanEntity.CBsmvRate;
                var orjCKkdfRate = loanEntity.CKkdfRate;
                var orjCKdvRate = loanEntity.CKdvRate;

                //Yeni Kar Oranıyla Hesapla
                ////////////////////////////////////////////////////////
                loanEntity.CProfitRate = (double)loanEntity.CIncentiveAppliedProfitRate;
                loanEntity = PrepareScheduleTable(loanEntity);
                ////////////////////////////////////////////////////////

                loanEntity.CAppliedProfitRate = (double)loanEntity.CIncentiveAppliedProfitRate;
                loanEntity.CIncentiveAmount = (orjCProfitAmount - loanEntity.CProfitAmount) /
                    ((1.0d / (1 + orjCBsmvRate + orjCKkdfRate + orjCKdvRate)) + ((orjCProfitAmount / orjCPrincipalAmount) * (1.0d / (1 + orjCBsmvRate + orjCKkdfRate + orjCKdvRate))));
                loanEntity.CIncentiveRate = (loanEntity.CIncentiveAmount / orjCPrincipalAmount) * 100.0d;
            }

            return loanEntity;
        }

        /// <summary>
        /// Prepares the schedule table.
        /// Taksitleri Hesapla
        /// </summary>
        /// <returns>The schedule table.</returns>
        /// <param name="loanEntity">Loan entity.</param>
        private LoanEntity PrepareScheduleTable(LoanEntity loanEntity)
        {
            //TODO if CType = 'L' -> Leasing için taksit hesabı değişiyor ona göre düzenleme yapılmalı

            var periodicPaymentAmount = RoundAway(loanEntity.CPrincipalAmount * (loanEntity.CProfitRate / 100.0d)
                * (1 + loanEntity.CBsmvRate + loanEntity.CKkdfRate + loanEntity.CKdvRate)
                * Math.Pow(1 + (loanEntity.CProfitRate / 100.0d) * (1 + loanEntity.CBsmvRate + loanEntity.CKkdfRate + loanEntity.CKdvRate), loanEntity.CInstallmentCount)
                / (Math.Pow(1 + (loanEntity.CProfitRate / 100.0d) * (1 + loanEntity.CBsmvRate + loanEntity.CKkdfRate + loanEntity.CKdvRate), loanEntity.CInstallmentCount) - 1), 2);

            var smallestBigPeriodicPaymentAmount = 0d;
            var biggestSmallPeriodicPaymentAmount = 0d;
            var lastPrincipleAmount = 0d;
            var lastPrincipleBalance = 0d;
            var amountList = new List<double>(); // Toplam Taksit Tutarı için Denemelerin Tutulduğu liste
            do
            {
                if (Math.Abs(biggestSmallPeriodicPaymentAmount) <= EPSILON && Math.Abs(smallestBigPeriodicPaymentAmount) <= EPSILON)
                {
                    // ilk değer kalsın -> periodicPaymentAmount = periodicPaymentAmount 
                }
                else if (biggestSmallPeriodicPaymentAmount > 0 && Math.Abs(smallestBigPeriodicPaymentAmount) <= EPSILON)
                {
                    // oransal yakınsama
                    periodicPaymentAmount = RoundAway(periodicPaymentAmount + ((lastPrincipleBalance - lastPrincipleAmount) / (loanEntity.CInstallmentCount)), 2);
                }
                else if (Math.Abs(biggestSmallPeriodicPaymentAmount) <= EPSILON && smallestBigPeriodicPaymentAmount > 0)
                {
                    // oransal yakınsama
                    periodicPaymentAmount = RoundAway(periodicPaymentAmount + ((lastPrincipleBalance - lastPrincipleAmount) / (loanEntity.CInstallmentCount)), 2);
                }
                else
                {
                    var newAmount = RoundAway((biggestSmallPeriodicPaymentAmount + smallestBigPeriodicPaymentAmount) / 2.0d, 2);
                    //bi önceki değerin aynısını alıcaksa yinelemesin çıksın artık
                    if (Math.Abs(periodicPaymentAmount - newAmount) <= EPSILON)
                    {
                        break;
                    }
                    // oransal yakınsama
                    periodicPaymentAmount = newAmount;
                }

                var principalBalance = loanEntity.CPrincipalAmount;
                var prevDeferredProfit = 0d;
                loanEntity.CProfitAmount = 0d;
                loanEntity.CBsmvAmount = 0d;
                loanEntity.CKkdfAmount = 0d;
                loanEntity.CKdvAmount = 0d;
                foreach (var row in loanEntity.AmortizationSchedule)
                {
                    //Taksit Tutarı Sabit Değilse Hesaplanan Taksit tutarını al
                    if (!row.IFixity.HasFlag(FixityTypes.InstallmentAmount))
                    {
                        row.IAmount = periodicPaymentAmount;
                    }
                    //Kar Oranı Sabit Değilse Kredi Kar oranını al
                    if (!row.IFixity.HasFlag(FixityTypes.ProfitRate))
                    {
                        row.IProfitRate = loanEntity.CProfitRate;
                    }
                    //Bsmv Oranı Sabit Değilse Kredi Bsmv oranını al
                    if (!row.IFixity.HasFlag(FixityTypes.BsmvRate))
                    {
                        row.IBsmvRate = loanEntity.CBsmvRate;
                    }
                    //Kkdf Oranı Sabit Değilse Kredi Kkdf oranını al
                    if (!row.IFixity.HasFlag(FixityTypes.KkdfRate))
                    {
                        row.IKkdfRate = loanEntity.CKkdfRate;
                    }
                    //Kdv Oranı Sabit Değilse Kredi Kdv oranını al
                    if (!row.IFixity.HasFlag(FixityTypes.KdvRate))
                    {
                        row.IKdvRate = loanEntity.CKdvRate;
                    }

                    row.IPrincipalBalance = RoundAway(principalBalance, 2);
                    var dayNumber = (row.IMaturityDate - row.IValueDate).Days;
                    row.IProfitAmount = RoundAway(prevDeferredProfit + RoundAway(row.IPrincipalBalance * (row.IProfitRate / 100.0d) * (dayNumber / 30.0d), 2), 2);
                    row.IBsmvAmount = RoundAway(row.IProfitAmount * row.IBsmvRate, 2);
                    row.IKkdfAmount = RoundAway(row.IProfitAmount * row.IKkdfRate, 2);
                    row.IKdvAmount = RoundAway(row.IProfitAmount * row.IKdvRate, 2);
                    //Eğer kar + vergiler taksit tutarını geçecekse kar + vergiler = taksit tutarı olacak şekilde ayarla ve anaparıyı 0'a çek
                    var supposedProfit = row.IProfitAmount; //olması gereken kar
                    if (RoundAway(row.IProfitAmount + row.IBsmvAmount + row.IKkdfAmount + row.IKdvAmount, 2) > row.IAmount)
                    {
                        row.IProfitAmount = RoundAway(row.IAmount / (1 + row.IBsmvRate + row.IKkdfRate + row.IKdvRate), 2);
                        row.IBsmvAmount = RoundAway(row.IProfitAmount * row.IBsmvRate, 2);
                        row.IKkdfAmount = RoundAway(row.IProfitAmount * row.IKkdfRate, 2);
                        row.IKdvAmount = RoundAway(row.IProfitAmount * row.IKdvRate, 2);
                        // Yuvarlama sıkıntısı olmasın diye(profit+bsmv+kkdf+kdv=taksitTutarı olsunDiye)
                        row.IProfitAmount = RoundAway(row.IAmount - (row.IBsmvAmount + row.IKkdfAmount + row.IKdvAmount), 2);
                        row.IPrincipalAmount = 0d;
                    }
                    else
                    {
                        row.IPrincipalAmount = RoundAway(row.IAmount - (row.IProfitAmount + row.IBsmvAmount + row.IKkdfAmount + row.IKdvAmount), 2);
                    }
                    principalBalance = RoundAway(principalBalance - row.IPrincipalAmount, 2);

                    prevDeferredProfit = RoundAway(supposedProfit - row.IProfitAmount, 2);
                    row.IDeferredProfit = prevDeferredProfit;

                    loanEntity.CProfitAmount += row.IProfitAmount;
                    loanEntity.CBsmvAmount += row.IBsmvAmount;
                    loanEntity.CKkdfAmount += row.IKkdfAmount;
                    loanEntity.CKdvAmount += row.IKdvAmount;
                }

                lastPrincipleAmount = loanEntity.AmortizationSchedule[loanEntity.CInstallmentCount - 1].IPrincipalAmount;
                lastPrincipleBalance = loanEntity.AmortizationSchedule[loanEntity.CInstallmentCount - 1].IPrincipalBalance;

                if (lastPrincipleAmount > lastPrincipleBalance)
                {
                    smallestBigPeriodicPaymentAmount = periodicPaymentAmount;
                }
                else
                {
                    biggestSmallPeriodicPaymentAmount = periodicPaymentAmount;
                }

                amountList.Add(periodicPaymentAmount);

                // Son taksit anaparası kalan anaparaya eşit gibi olana kadar dön. Fark 1 veya 1den küçükse veya döngü sayısı 100'ü geçerse çık artık
            } while (Math.Abs(lastPrincipleAmount - lastPrincipleBalance) > 1 && amountList.Count <= 100);

            return loanEntity;
        }

        /// <summary>
        /// RoundAway'u  MidpointRounding.AwayFromZero ile kullan
        /// </summary>
        /// <returns>Rounded value</returns>
        /// <param name="value">Value.</param>
        /// <param name="digits">Digits.</param>
        private double RoundAway(double value, int digits)
        {
            return Math.Round(value, digits, MidpointRounding.AwayFromZero);
        }
    }
}
