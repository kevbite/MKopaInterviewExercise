# M-KOPA Interview Exercise

This code contains a message handler (`SendSmsConsumer`) that consumes a `SendSms` message, the handler tries to handle the message only once by keeping track of message that have already been processed correlating based on its `MessageId`.

There's some basic tests around the handler that test the behaviors and these can be run with the following command from the root directory:

```bash
dotnet test
```