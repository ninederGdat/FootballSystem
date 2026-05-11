using Supabase;
using System;
using System.Collections.Generic;
using System.Text;

namespace FotmobSync.Infrastructure
{
    public class SupabaseClientFactory
    {
        private readonly IConfiguration _configuration;
        private readonly Lazy<Supabase.Client> _anoClient;
        private readonly Lazy<Supabase.Client> _serviceRoleClient;

        public SupabaseClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            _anoClient = new Lazy<Supabase.Client>(() => CreateClient(useServiceRole: false));
            _serviceRoleClient = new Lazy<Supabase.Client>(() => CreateClient(useServiceRole: true));
        }


        public Supabase.Client CreateAnonClient() => _anoClient.Value;

        public Supabase.Client CreateServiceRoleClient() => _serviceRoleClient.Value;

        private Supabase.Client CreateClient(bool useServiceRole)
        {
            var supabaseUrl = _configuration["Supabase:Url"];
            var supabaseKey = useServiceRole ? _configuration["Supabase:ServiceRoleKey"] : _configuration["Supabase:AnonKey"];
            if (string.IsNullOrWhiteSpace(supabaseKey))
            {
                throw new InvalidOperationException(
                    useServiceRole
                        ? "Supabase:ServiceRoleKey is not configured"
                        : "Supabase:AnonKey is not configured");
            }
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true,
                // Thêm các tùy chọn khác nếu cần
            };
            return new Supabase.Client(supabaseUrl, supabaseKey, options);
        }

        private object CreateServiceRoleClient(bool useServiceRole)
        {
            throw new NotImplementedException();
        }
    }
}
