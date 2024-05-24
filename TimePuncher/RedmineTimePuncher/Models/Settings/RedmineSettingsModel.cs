using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class RedmineSettingsModel : Bases.SettingsModelBase<RedmineSettingsModel>
    {
        public LocaleType Locale { get; set; }

        public string UrlBase { get; set; }

        [JsonIgnore]
        public string UserName { get; set; }
        [JsonIgnore]
        public string Password 
        { 
            get => PasswordEncrypt.Decrypt();
            set => PasswordEncrypt = value.Encrypt();
        }
        [JsonIgnore]
        public string PasswordEncrypt { get; set; }
        [JsonIgnore]
        public string AdminApiKey { get; set; }

        public int ConcurrencyMax { get; set; } = 5;

        // Basic 認証用の設定
        public bool UseBasicAuth { get; set; }
        [JsonIgnore]
        public string ApiKey { get; set; }
        [JsonIgnore]
        public string UserNameOfBasicAuth { get; set; }
        [JsonIgnore]
        public string PasswordOfBasicAuth
        {
            get => PasswordEncryptOfBasicAuth.Decrypt();
            set => PasswordEncryptOfBasicAuth = value.Encrypt();
        }
        [JsonIgnore]
        public string PasswordEncryptOfBasicAuth { get; set; }

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> IsValid { get; }

        public RedmineSettingsModel()
        {
            var isValid = new[]
            {
                this.ObserveProperty(a => a.UrlBase),
                this.ObserveProperty(a => a.UserName),
                this.ObserveProperty(a => a.Password),
                this.ObserveProperty(a => a.AdminApiKey).Select(_ => "temp"),
            }.CombineLatest(a => a.All(b => !string.IsNullOrEmpty(b)));

            var isValidBasic = new[]
            {
                this.ObserveProperty(a => a.ApiKey),
                this.ObserveProperty(a => a.UserNameOfBasicAuth),
                this.ObserveProperty(a => a.PasswordOfBasicAuth),
            }.CombineLatest(a => a.All(b => !string.IsNullOrEmpty(b)));

            IsValid = isValid.CombineLatest(this.ObserveProperty(a => a.UseBasicAuth), isValidBasic,
                                           (iv1, useBasic, iv2) => !useBasic ? iv1 : iv1 && iv2)
                             .ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
                             .AddTo(disposables);
        }

        // [IgnoreDataMember] のプロパティがあるため ToJson で同値判定ができない。よって Equals を override する。
        public override bool Equals(object obj)
        {
            if(obj is RedmineSettingsModel model)
            {
                return Locale == model.Locale &&
                       UrlBase == model.UrlBase &&
                       UserName == model.UserName &&
                       Password == model.Password &&
                       PasswordEncrypt == model.PasswordEncrypt &&
                       AdminApiKey == model.AdminApiKey &&
                       ConcurrencyMax == model.ConcurrencyMax &&
                       UseBasicAuth == model.UseBasicAuth &&
                       ApiKey == model.ApiKey &&
                       UserNameOfBasicAuth == model.UserNameOfBasicAuth &&
                       PasswordOfBasicAuth == model.PasswordOfBasicAuth &&
                       (
                           string.IsNullOrEmpty(PasswordEncryptOfBasicAuth) == string.IsNullOrEmpty(model.PasswordEncryptOfBasicAuth) || 
                           PasswordEncryptOfBasicAuth == model.PasswordEncryptOfBasicAuth
                       );
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 877536718;
            hashCode = hashCode * -1521134295 + Locale.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UrlBase);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Password);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PasswordEncrypt);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AdminApiKey);
            hashCode = hashCode * -1521134295 + ConcurrencyMax.GetHashCode();
            hashCode = hashCode * -1521134295 + UseBasicAuth.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ApiKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserNameOfBasicAuth);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PasswordOfBasicAuth);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PasswordEncryptOfBasicAuth);
            return hashCode;
        }
    }
}
