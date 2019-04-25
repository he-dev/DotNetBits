using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

#if NET47
using System.Net.Configuration;
#endif

namespace Reusable.IOnymous
{
    [PublicAPI]
    public static class MailProviderMetadataExtensions
    {
        public static MetadataScope<MailProvider> Mail(this Metadata metadata)
        {
            return metadata.For<MailProvider>();
        }

        public static Metadata Mail(this Metadata metadata, ConfigureMetadataScopeCallback<MailProvider> scope)
        {
            return metadata.For(scope);
        }

        public static IEnumerable<string> To(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetItemByCallerName(Enumerable.Empty<string>());
        }

        public static MetadataScope<MailProvider> To(this MetadataScope<MailProvider> scope, IEnumerable<string> to)
        {
            return scope.Metadata.SetItemByCallerName(to);
        }

        // --- 

        public static IEnumerable<string> CC(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetItemByCallerName(Enumerable.Empty<string>());
        }

        public static MetadataScope<MailProvider> CC(this MetadataScope<MailProvider> scope, IEnumerable<string> cc)
        {
            return scope.Metadata.SetItemByCallerName(cc);
        }

        // ---

        public static string From(this MetadataScope<MailProvider> scope)
        {
#if NETCOREAPP2_2
            return scope.Metadata.GetItemByCallerName(string.Empty);
#endif
#if NET47
            return scope.Metadata.GetItemByCallerName(Default());

            string Default()
            {
                const string mailSectionGroupName = "system.net/mailSettings";
                var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mailSettingsSectionGroup = configuration.GetSectionGroup(mailSectionGroupName) as MailSettingsSectionGroup;
                if (mailSettingsSectionGroup?.Smtp?.From is null)
                {
                    throw new InvalidOperationException
                    (
                        $"You didn't specify {nameof(From)} email and there is no default value in the '{mailSectionGroupName}' section in the app.config."
                    );
                }

                return mailSettingsSectionGroup.Smtp.From;
            }
#endif
        }

        public static MetadataScope<MailProvider> From(this MetadataScope<MailProvider> scope, string from)
        {
            return scope.Metadata.SetItemByCallerName(from);
        }

        // ---

        public static string Subject(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetItemByCallerName(string.Empty);
        }

        public static MetadataScope<MailProvider> Subject(this MetadataScope<MailProvider> scope, string subject)
        {
            return scope.Metadata.SetItemByCallerName(subject);
        }

        // ---

        public static Encoding SubjectEncoding(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetItemByCallerName(Encoding.UTF8);
        }

        public static MetadataScope<MailProvider> SubjectEncoding(this MetadataScope<MailProvider> scope, string subjectEncoding)
        {
            return scope.Metadata.SetItemByCallerName(subjectEncoding);
        }

        public static Dictionary<string, byte[]> Attachments(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetItemByCallerName(new Dictionary<string, byte[]>());
        }

        public static MetadataScope<MailProvider> Attachments(this MetadataScope<MailProvider> scope, Dictionary<string, byte[]> attachments)
        {
            return scope.Metadata.SetItemByCallerName(attachments);
        }

        // ---

        //        public static string Body(this ResourceMetadataScope<MailProvider> scope)
        //        {
        //            return scope.Metadata.GetValueOrDefault(string.Empty);            
        //        }
        //
        //        public static ResourceMetadataScope<MailProvider> Body(this ResourceMetadataScope<MailProvider> scope, string body)
        //        {
        //            return scope.Metadata.SetItemAuto(body);
        //        }

        // ---

        public static Encoding BodyEncoding(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetItemByCallerName(Encoding.UTF8);
        }

        public static MetadataScope<MailProvider> BodyEncoding(this MetadataScope<MailProvider> scope, string bodyEncoding)
        {
            return scope.Metadata.SetItemByCallerName(bodyEncoding);
        }

        // ---

        public static bool IsHtml(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetItemByCallerName(true);
        }

        public static MetadataScope<MailProvider> IsHtml(this MetadataScope<MailProvider> scope, bool isBodyHtml)
        {
            return scope.Metadata.SetItemByCallerName(isBodyHtml);
        }

        // ---

        public static bool IsHighPriority(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetItemByCallerName(false);
        }

        public static MetadataScope<MailProvider> IsHighPriority(this MetadataScope<MailProvider> scope, bool isHighPriority)
        {
            return scope.Metadata.SetItemByCallerName(isHighPriority);
        }

        // ---
    }
}