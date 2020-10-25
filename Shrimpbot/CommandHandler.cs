﻿using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using LiteDB;
using Shrimpbot.Services;
using Discord;
using Discord.Addons.Interactive;

namespace Shrimpbot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;
        private readonly ConfigurationFile config;
        private readonly BotRuntimeInformation runtimeInformation;

        public CommandHandler(DiscordSocketClient client, CommandService commands, ConfigurationFile config, BotRuntimeInformation runtimeInformation)
        {
            this.commands = commands;
            this.client = client;
            this.config = config;
            this.runtimeInformation = runtimeInformation;

            services = new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton(client)
                .AddSingleton(runtimeInformation)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
        }

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;

            var argPos = 0;

            if (!(message.HasStringPrefix(config.Prefix, ref argPos)) || message.Author.IsBot) return;

            var context = new SocketCommandContext(client, message);

            var result = await commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: services);
            LoggingService.Log(LogSeverity.Verbose, $"Executed a command! {context.Guild.Name} at {context.Channel.Name}.");
            runtimeInformation.CommandsHandled++;
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync($"Oopsies! {result.ErrorReason}");
        }
    }
}
