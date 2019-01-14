﻿using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Crmf;

namespace Org.BouncyCastle.Cmp
{
   
    public class ProtectedPkiMessage
    {
        private PkiMessage pkiMessage;
        

        public ProtectedPkiMessage(GeneralPKIMessage pkiMessage)
        {
            
            if (!pkiMessage.HasProtection)
            {
                throw new ArgumentException("pki message not protected");
            }

            this.pkiMessage = pkiMessage.ToAsn1Structure();
        }
           
        public ProtectedPkiMessage(PkiMessage pkiMessage)
        {
            if (pkiMessage.Header.ProtectionAlg == null)
            {
                throw new ArgumentException("pki message not protected");
            }

            this.pkiMessage = pkiMessage;
        }

        public PkiHeader Header { get { return pkiMessage.Header; } }
        public PkiBody Body { get { return pkiMessage.Body; } }

        public PkiMessage ToAsn1Message() { return pkiMessage; }

        public bool HasPasswordBasedMacProtected { get { return Header.ProtectionAlg.Algorithm.Equals(CmpObjectIdentifiers.passwordBasedMac); } }

        public X509Certificate[] GetCertificates()
        {
            CmpCertificate[] certs = pkiMessage.GetExtraCerts();

            if (certs == null)
            {
                return new X509Certificate[0];
            }

            X509Certificate[] res = new X509Certificate[certs.Length];
           for (int t=0; t<certs.Length;t++)
            {
                res[t] = new X509Certificate(X509CertificateStructure.GetInstance(certs[t].GetEncoded()));
            }

            return res;
        }

        public bool Verify(IVerifierFactory verifierFactory)
        {
            IStreamCalculator streamCalculator = verifierFactory.CreateCalculator();

            IVerifier result = (IVerifier)Process(streamCalculator);

            return result.IsVerified(pkiMessage.Protection.GetBytes());
        }

        private Object Process(IStreamCalculator streamCalculator)
        {
           Asn1EncodableVector avec = new Asn1EncodableVector();
           avec.Add(pkiMessage.Header);
           avec.Add(pkiMessage.Body);
           byte[] enc =   new DerSequence(avec).GetDerEncoded();

           streamCalculator.Stream.Write(enc,0,enc.Length);
           streamCalculator.Stream.Flush();
           streamCalculator.Stream.Close();
          
           return streamCalculator.GetResult();          
        }

        public bool Verify(PKMacBuilder pkMacBuilder, char[] password)
        {
            if (!CmpObjectIdentifiers.passwordBasedMac.Equals(pkiMessage.Header.ProtectionAlg.Algorithm))
            {
                throw new InvalidOperationException("protection algorithm is not mac based");
            }

            PbmParameter parameter = PbmParameter.GetInstance(pkiMessage.Header.ProtectionAlg.Parameters);

            pkMacBuilder.SetParameters(parameter);

            IBlockResult result = (IBlockResult)Process(pkMacBuilder.Build(password).CreateCalculator());

            return Arrays.ConstantTimeAreEqual(result.Collect(), this.pkiMessage.Protection.GetBytes());
        }
    }
}
