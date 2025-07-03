# Maelstrom.NET

A C#/.NET implementation of a [Maelstrom](https://github.com/jepsen-io/maelstrom/tree/main) server and workloads that cover the challenges in the [Fly.io Gossip Gloomers](https://fly.io/dist-sys/) set of distributed systems challenges.

# Setup
1. put `maelstrom.jar` to `lib` folder
2. run `make build`
3. run `make echo` to start the echo example. see `Makefile` which arguments are used and what other make targets are available.