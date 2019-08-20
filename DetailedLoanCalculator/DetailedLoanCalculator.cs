using System;
using System.Collections.Generic;
using System.Globalization;

using Albaraka.Utils.Calculator.LoanCalculator.Entity;
using Albaraka.Utils.Calculator.LoanCalculator.Utility;

namespace Albaraka.Utils.Calculator.LoanCalculator
{
    /// <summary>
    /// Detailed loan calculator.
    /// </summary>
    public class DetailedLoanCalculator
    {
        private LoanEntity loanEntity;
        private CultureInfo culture;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Albaraka.Utils.Calculator.LoanCalculator.DetailedLoanCalculator"/> class.
        /// </summary>
        /// <param name="loanEntity">Loan entity.</param>
        /// <param name="culture">Culture.</param>
        public DetailedLoanCalculator(LoanEntity loanEntity, CultureInfo culture)
        {
            if (this.loanEntity == null)
            {
                throw new ArgumentException(message: "Empty Argument: logEntry!");
            }
            this.loanEntity = loanEntity;

            if (culture == null)
            {
                this.culture = new CultureInfo("tr-TR");
            }
            else
            {
                this.culture = culture;
            }
        }

        /// <summary>
        /// Calculate Loan.
        /// </summary>
        /// <returns>Calculated LoanEntity Object.</returns>
        public LoanEntity Calculate()
        {
            var IDate = this.loanEntity.CValueDate;
            for (int i = 0; i < this.loanEntity.CInstallmentCount; i++)
            {
                this.loanEntity.AmortizationSchedule.Add(new AmortizationScheduleRow
                {
                    IRowNumber = i + 1,
                    IValueDate = i == 0 ? this.loanEntity.CValueDate : DateUtilities.AddMonth(this.loanEntity.CFirstInstallmentDate, i - 1),
                    IMaturityDate = DateUtilities.AddMonth(this.loanEntity.CFirstInstallmentDate, i),
                    IProfitRate = this.loanEntity.CProfitRate,
                    IBsmvRate = this.loanEntity.CBsmvRate,
                    IKkdfRate = this.loanEntity.CKkdfRate,
                    IKdvRate = this.loanEntity.CKdvRate
                });
            }

            //this.loanEntity.AmortizationSchedule.FirstOrDefault(s => s.IRowNumber == 1).IFixity = FixityTypes.InstallmentAmount;
            //this.loanEntity.AmortizationSchedule.FirstOrDefault(s => s.IRowNumber == 1).IAmount = 500000;

            //Taksit Tarihlerini iş gününe geliyorsa ötele
            foreach (var row in this.loanEntity.AmortizationSchedule)
            {
                if (row.IRowNumber > 1)
                {
                    row.IValueDate = DateUtilities.GetBusinessDay(row.IValueDate, culture);
                }
                row.IMaturityDate = DateUtilities.GetBusinessDay(row.IMaturityDate, culture);
            }

            var installment = new Installment();
            return installment.CalculateInstallments(this.loanEntity);
        }
    }
}
