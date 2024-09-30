using AutoMapper;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
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

        #region "UserName"
        [JsonIgnore]
        public string UserName
        {
            get => UserNameEncrypt.Decrypt();
            set => UserNameEncrypt = value.Encrypt();
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Nullだったら項目自体を出力しない
        public string UserNameEncrypt { get; set; }
        #endregion

        #region "Password"
        [JsonIgnore]
        public string Password 
        {
            get => PasswordEncrypt.Decrypt();
            set => PasswordEncrypt = value.Encrypt();
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Nullだったら項目自体を出力しない
        public string PasswordEncrypt { get; set; }
        #endregion

        #region "AdminApiKey"
        [JsonIgnore]
        public string AdminApiKey
        {
            get => AdminApiKeyEncrypt.Decrypt();
            set => AdminApiKeyEncrypt = value.Encrypt();
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Nullだったら項目自体を出力しない
        public string AdminApiKeyEncrypt { get; set; }
        #endregion

        public int ConcurrencyMax { get; set; } = 5;

        // Basic 認証用の設定
        public bool UseBasicAuth { get; set; }
        #region "ApiKey"
        [JsonIgnore]
        public string ApiKey
        {
            get => ApiKeyEncrypt.Decrypt();
            set => ApiKeyEncrypt = value.Encrypt();
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Nullだったら項目自体を出力しない
        public string ApiKeyEncrypt { get; set; }
        #endregion

        #region "UserNameOfBasicAuth"
        [JsonIgnore]
        public string UserNameOfBasicAuth
        {
            get => UserNameOfBasicAuthEncrypt.Decrypt();
            set => UserNameOfBasicAuthEncrypt = value.Encrypt();
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Nullだったら項目自体を出力しない
        public string UserNameOfBasicAuthEncrypt { get; set; }
        #endregion

        #region "PasswordOfBasicAuth"
        [JsonIgnore]
        public string PasswordOfBasicAuth
        {
            get => PasswordEncryptOfBasicAuth.Decrypt();
            set => PasswordEncryptOfBasicAuth = value.Encrypt();
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Nullだったら項目自体を出力しない
        public string PasswordEncryptOfBasicAuth { get; set; }
        #endregion

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

        public override void Export(string fileName)
        {
            // ユーザーの接続情報は、エクスポートしない
            var temp = this.Clone();
            temp.SetNullValueForExport();

            // Nullは、出力しないようにすることで、エクスポート情報から項目自体を外す。
            // また、本ファイルをインポートしたとしても、Nullの値は上書きされない。
            FileHelper.WriteAllText(fileName, temp.ToJson());
        }

        /// <summary>
        /// エクスポートを行う際に、接続情報が含まれないようにNullクリアを行う。
        /// </summary>
        public void SetNullValueForExport()
        {
            UserName = null;
            PasswordEncrypt = null;
            AdminApiKey = null;
            ApiKey = null;
            UserNameOfBasicAuth = null;
            PasswordEncryptOfBasicAuth = null;
        }

        public override bool Equals(object obj)
        {
            if (obj is RedmineSettingsModel model)
            {
                return Locale == model.Locale &&
                       UrlBase == model.UrlBase &&
                       UserName == model.UserName &&
                       Password == model.Password &&
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
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AdminApiKey);
            hashCode = hashCode * -1521134295 + ConcurrencyMax.GetHashCode();
            hashCode = hashCode * -1521134295 + UseBasicAuth.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ApiKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserNameOfBasicAuth);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PasswordOfBasicAuth);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PasswordEncryptOfBasicAuth);
            return hashCode;
        }

        [Obsolete("V8.2.2からは、Properties.Settings.Defaultからの読み込みは不要となる。(#1658)", false)]
        /// <summary>
        /// ユーザ名などはエクスポートの対象外とするため [JsonIgnore] に設定している。よって個別に Load する。
        /// </summary>
        public void LoadProperties()
        {
            UserName                   = Properties.Settings.Default.UserName;
            PasswordEncrypt            = Properties.Settings.Default.Password;
            AdminApiKey                = Properties.Settings.Default.AdminApiKey;
            ApiKey                     = Properties.Settings.Default.ApiKey;
            UserNameOfBasicAuth        = Properties.Settings.Default.UserNameOfBasicAuth;
            PasswordEncryptOfBasicAuth = Properties.Settings.Default.PasswordEncryptOfBasicAuth;
        }

        public override void SetupConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<RedmineSettingsModel, RedmineSettingsModel>()
                .ForMember(m => m.UserName, o => o.Ignore())
                .ForMember(m => m.Password, o => o.Ignore())
                .ForMember(m => m.PasswordEncrypt, o => o.Ignore())
                .ForMember(m => m.AdminApiKey, o => o.Ignore())
                .ForMember(m => m.ApiKey, o => o.Ignore())
                .ForMember(m => m.UserNameOfBasicAuth, o => o.Ignore())
                .ForMember(m => m.PasswordOfBasicAuth, o => o.Ignore())
                .ForMember(m => m.PasswordEncryptOfBasicAuth, o => o.Ignore());
        }
    }
}
