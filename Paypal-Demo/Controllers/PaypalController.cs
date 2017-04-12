using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Paypal_Demo.Models;
using PayPal.Api;

namespace Paypal_Demo.Controllers
{
    public class PaypalController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PaymentWithCreditCard()
        {
            var item = new Item
            {
                name = "Demo Item",
                currency = "USD",
                price = "5",
                quantity = "1",
                sku = "sku"
            };

            var itms = new List<Item> {item};
            var itemList = new ItemList {items = itms};

            var billingAddress = new Address
            {
                city = "NewYork",
                country_code = "US",
                line1 = "23rd street kew gardens",
                postal_code = "43210",
                state = "NY"
            };

            var crdtCard = new CreditCard
            {
                billing_address = billingAddress,
                cvv2 = "874",
                expire_month = 1,
                expire_year = 2020,
                first_name = "Aman",
                last_name = "Thakur",
                number = "1234567890123456",
                type = "visa"
            };

            var details = new Details
            {
                shipping = "1",
                subtotal = "5",
                tax = "1"
            };

            var amnt = new Amount
            {
                currency = "USD",
                total = "7",
                details = details
            };

            var tran = new Transaction
            {
                amount = amnt,
                description = "Description about the payment amount.",
                item_list = itemList,
                invoice_number = "1"
            };

            var transactions = new List<Transaction> {tran};

            var fundInstrument = new FundingInstrument {credit_card = crdtCard};

            var fundingInstrumentList = new List<FundingInstrument> {fundInstrument};

            var payr = new Payer
            {
                funding_instruments = fundingInstrumentList,
                payment_method = "credit_card"
            };

            var pymnt = new Payment
            {
                intent = "sale",
                payer = payr,
                transactions = transactions
            };

            try
            {
                var apiContext = Configuration.GetApiContext();
                var createdPayment = pymnt.Create(apiContext);
                if (createdPayment.state.ToLower() != "approved")
                {
                    return View();
                }
            }
            catch (PayPal.PayPalException ex)
            {
                throw ex;
            }

            return View();
        }

        public ActionResult PaymentWithPaypal()
        {
            var apiContext = Configuration.GetApiContext();

            try
            {
                var payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    if (Request.Url != null)
                    {
                        var baseUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/Paypal/PaymentWithPayPal?";

                        var guid = Convert.ToString((new Random()).Next(100000));

                        var createdPayment = CreatePayment(apiContext, baseUri + "guid=" + guid);


                        var links = createdPayment.links.GetEnumerator();

                        string paypalRedirectUrl = null;

                        while (links.MoveNext())
                        {
                            var lnk = links.Current;

                            if (lnk != null && lnk.rel.ToLower().Trim().Equals("approval_url"))
                            {
                                paypalRedirectUrl = lnk.href;
                            }
                        }
                        Session.Add(guid, createdPayment.id);
                        return Redirect(paypalRedirectUrl);
                    }
                }
                else
                {

                    var guid = Request.Params["guid"];

                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);

                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View();
                    }

                }
            }
            catch (Exception ex)
            {
                return View();
            }

            return View();
        }

        private Payment _payment;

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution { payer_id = payerId };
            _payment = new Payment { id = paymentId };
            return _payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var itemList = new ItemList() { items = new List<Item>() };

            itemList.items.Add(new Item()
            {
                name = "Item Name",
                currency = "USD",
                price = "5",
                quantity = "1",
                sku = "sku"
            });

            var payer = new Payer { payment_method = "paypal" };

            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            var details = new Details
            {
                tax = "1",
                shipping = "1",
                subtotal = "5"
            };

            var amount = new Amount()
            {
                currency = "USD",
                total = "7", 
                details = details
            };

            var transactionList = new List<Transaction>
            {
                new Transaction
                {
                    description = "Transaction description.",
                    invoice_number = "your invoice number",
                    amount = amount,
                    item_list = itemList
                }
            };


            _payment = new Payment
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

           
            return _payment.Create(apiContext);

        }
    }
}
