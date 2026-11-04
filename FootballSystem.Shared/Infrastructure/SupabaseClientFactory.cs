using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballSystem.Shared.Infrastructure;

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

        if (PgDebugEnabled)
        {
            // Chỉ log "hình dạng" của URL/key, không bao giờ log giá trị key.
            Console.WriteLine(
            $"[PG-DEBUG] client={(useServiceRole ? "service_role" : "anon")} " +
            $"url={supabaseUrl} keyLen={supabaseKey.Length} keyPrefix={supabaseKey[..Math.Min(8, supabaseKey.Length)]} " +
            $"keyHasWhitespace={supabaseKey != supabaseKey.Trim()}");
        }

        // ETL worker: no realtime WebSocket or token refresh — faster first DB use at job time.
        var options = new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };
        var client = new Supabase.Client(supabaseUrl, supabaseKey, options);

        if (PgDebugEnabled)
            RegisterPgDebugHook(client);

        return client;
    }

    // TODO(debug): tạm thời, xoá sau khi tìm ra nguyên nhân PGRST102 trên GitHub Actions.
    // Bật bằng biến môi trường PG_DEBUG=1.
    private static bool PgDebugEnabled => Environment.GetEnvironmentVariable("PG_DEBUG") == "1";

    private static int _pgDebugHookRegistered;

    private static void RegisterPgDebugHook(Supabase.Client client)
    {
        // Hooks.Instance là singleton toàn process → chỉ đăng ký một lần.
        if (Interlocked.Exchange(ref _pgDebugHookRegistered, 1) == 1)
            return;

        client.Postgrest.AddRequestPreparedHandler(
            (sender, clientOptions, method, url, serializerSettings, data, headers) =>
             {
                 // Chỉ log method, URL, body và các header an toàn — KHÔNG log apikey/Authorization.
                 var body = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                 headers.TryGetValue("Prefer", out var prefer);
                 Console.WriteLine(
                     $"[PG-DEBUG] {method} {url} prefer='{prefer}' bodyLen={body.Length} body={body}");
             });
    }
}

