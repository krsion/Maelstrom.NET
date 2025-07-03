build:
	dotnet publish -c Release -o ./bin -r linux-x64 --self-contained true

serve:
	java -jar lib/maelstrom.jar serve

echo:
	java -jar lib/maelstrom.jar test -w echo --bin "bin/EchoService" --time-limit 5

counter:
	java -jar lib/maelstrom.jar test -w pn-counter --bin "bin/CounterService" --time-limit 30 --rate 10 --nemesis partition