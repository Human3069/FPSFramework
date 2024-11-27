/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Tls
{
    public abstract class AbstractTlsSigner
        :   TlsSigner
    {
        protected TlsContext mContext;

        public virtual void Init(TlsContext context)
        {
            this.mContext = context;
        }

        public virtual byte[] GenerateRawSignature(AsymmetricKeyParameter privateKey, byte[] md5AndSha1)
        {
            return GenerateRawSignature(null, privateKey, md5AndSha1);
        }

        public abstract byte[] GenerateRawSignature(SignatureAndHashAlgorithm algorithm,
            AsymmetricKeyParameter privateKey, byte[] hash);

        public virtual bool VerifyRawSignature(byte[] sigBytes, AsymmetricKeyParameter publicKey, byte[] md5AndSha1)
        {
            return VerifyRawSignature(null, sigBytes, publicKey, md5AndSha1);
        }

        public abstract bool VerifyRawSignature(SignatureAndHashAlgorithm algorithm, byte[] sigBytes,
            AsymmetricKeyParameter publicKey, byte[] hash);

        public virtual ISigner CreateSigner(AsymmetricKeyParameter privateKey)
        {
            return CreateSigner(null, privateKey);
        }

        public abstract ISigner CreateSigner(SignatureAndHashAlgorithm algorithm, AsymmetricKeyParameter privateKey);

        public virtual ISigner CreateVerifyer(AsymmetricKeyParameter publicKey)
        {
            return CreateVerifyer(null, publicKey);
        }

        public abstract ISigner CreateVerifyer(SignatureAndHashAlgorithm algorithm, AsymmetricKeyParameter publicKey);

        public abstract bool IsValidPublicKey(AsymmetricKeyParameter publicKey);
    }
}

#endif
