using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using PackageStatus.Models;

namespace PackageStatus
{
    public class PackageService
    {
        public Package LocatePackage(string packageId)
        {
            Thread.Sleep(10000);

            if (packageId == "1234")
            {
                return new Package() { Status = "intransit" };
            }
            else if (packageId == "6789")
            {
                Thread.Sleep(10000);
                throw new FakeTimeoutException();
            }
            else
            {
                return null;
            }
        }
    }

    public class FakeTimeoutException : Exception
    {
    }

}