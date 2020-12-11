using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenModelMetadataType
{
    public class CommandRunner
    {
        private readonly List<CommandOption> _optionDescriptors;
        private Func<IDictionary<string, string>, int> _runFunc;
        private readonly List<CommandRunner> _subRunners;
        private readonly TextWriter _output;

        public CommandRunner(string commandName, string commandDescription, TextWriter output)
        {
            CommandName = commandName;
            CommandDescription = commandDescription;
            _optionDescriptors = new List<CommandOption>();
            _runFunc = (namedArgs) => { return 1; }; // noop
            _subRunners = new List<CommandRunner>();
            _output = output;
        }

        public string CommandName { get; private set; }

        public string CommandDescription { get; private set; }

        public void Option(string valueName, string longName, string shortName, string description)
        {
            _optionDescriptors.Add(new CommandOption
            {
                ValueName = valueName,
                LongName = longName,
                ShortName = shortName,
                Description = description
            });
        }

        public void OnRun(Func<IDictionary<string, string>, int> runFunc)
        {
            _runFunc = runFunc;
        }

        public void SubCommand(string name, string description, Action<CommandRunner> configAction)
        {
            var runner = new CommandRunner($"{CommandName} {name}", description, _output);
            configAction(runner);
            _subRunners.Add(runner);
        }

        public int Run(IEnumerable<string> args)
        {
            if (args.Any())
            {
                var subRunner = _subRunners.FirstOrDefault(r => r.CommandName.Split(' ').Last() == args.First());
                if (subRunner != null) return subRunner.Run(args.Skip(1));
            }

            if (_subRunners.Any() || !TryParseArgs(args, out IDictionary<string, string> namedArgs))
            {
                PrintUsage();
                return 1;
            }

            return _runFunc(namedArgs);
        }

        private bool TryParseArgs(IEnumerable<string> args, out IDictionary<string, string> namedArgs)
        {
            namedArgs = new Dictionary<string, string>();
            var argsQueue = new Queue<string>(args);

            // Process options first
            while (argsQueue.Any())
            {
                var name = argsQueue.Dequeue();

                var isLongOption = name.StartsWith("--", StringComparison.Ordinal);

                if (!isLongOption && !name.StartsWith("-", StringComparison.Ordinal))
                {
                    continue;
                }

                var optionPrefixLength = isLongOption ? 2 : 1;
                var optionName = name.Substring(optionPrefixLength);

                var option = _optionDescriptors.FirstOrDefault(d => isLongOption 
                    ? d.LongName == optionName 
                    : d.ShortName == optionName);

                if(option is null || string.IsNullOrWhiteSpace(argsQueue.Peek()))
                {
                    return false;
                }

                namedArgs.Add(option.ValueName, argsQueue.Dequeue());
            }

            return argsQueue.Count() == 0;
        }

        private void PrintUsage()
        {
            if (_subRunners.Any())
            {
                // List sub commands
                _output.WriteLine(CommandDescription);
                _output.WriteLine("Commands:");
                foreach (var runner in _subRunners)
                {
                    var shortName = runner.CommandName.Split(' ').Last();
                    if (shortName.StartsWith("_")) continue; // convention to hide commands
                    _output.WriteLine($"  {shortName}:  {runner.CommandDescription}");
                }
                _output.WriteLine();
            }
            else
            {
                // Usage for this command
                var optionsPart = _optionDescriptors.Any() ? "[options] " : "";
                _output.WriteLine($"Usage: {CommandName} {optionsPart}");
                _output.WriteLine();

                // Options
                if (_optionDescriptors.Any())
                {
                    _output.WriteLine("options:");
                    foreach (var option in _optionDescriptors)
                    {
                        _output.WriteLine($" --{option.LongName} | -{option.ShortName}:  {option.Description}");
                    }

                    _output.WriteLine();
                }
            }
        }
    }
}