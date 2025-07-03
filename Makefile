build:
	dotnet publish -c Release -o ./bin -r linux-x64 --self-contained true

echo:
	java -jar lib/maelstrom.jar test -w echo --bin "bin/EchoService" --time-limit 5