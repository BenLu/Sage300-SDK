// The MIT License (MIT) 
// Copyright (c) 1994-2016 The Sage Group plc or its licensors.  All rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in 
// the Software without restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
// Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


namespace ISV1.web.Areas.CU.DAL.ADO_EF.Model
{
    using Sage.CA.SBS.ERP.Sage300.Common.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    /// <summary>
    /// ARCustomer model class
    /// Can be generated by ADO.NET Entity Data Model by Code first. Refactoring to required entity class
    /// Todo: Use properties with backing fields instead of automatic properties on entities to trim blank. Retrieve DB char(n) column will have trailing blanks in EF 
    /// </summary>
    [Table("ARCUS")]
    public partial class ARCustomer 
    {
        [Key]
        [Column("IDCUST")]
        public string CustomerNumber { get ; set; }

        [Column("NAMECUST")]
        public string CustomerName { get; set; }

        [Column("EMAIL2")]
        public string Email { get; set; }

        [Column("TEXTPHON2")]
        public string FaxNumber { get; set; }

        [Column("NAMECTAC")]
        public string ContactName { get; set; }

        [Column("EMAIL1")]
        public string ContactsEmail { get; set; }

        [Column("IDACCTSET")]
        public string AccountSet { get; set; }

        [Column("WEBSITE")]
        public string WebSite { get; set; }

        [Column("BILLMETHOD")]
        public short BillMethod { get; set; }

        [Column("PAYMCODE")]
        public string PaymentCode { get; set; }

        public IList<ARCustomerOptionalField> ARCustomerOptionalFields { get; set; }
    }
}
