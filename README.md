# Distributed Event Deduplication for WebSocket Listeners

**Overview**

This project demonstrates a reliable, scalable event deduplication mechanism for a distributed system where multiple listener instances receive duplicate events over WebSocket connections.

The primary goal is to ensure that each event is processed and persisted logically once, even when:

- Multiple listeners receive the same event
- Listener instances crash or restart
- Events are delivered more than once
- Redis or network experiences transient failures

The solution is implemented in .NET and uses Redis for distributed coordination and a database for idempotent persistence.

**Alorithm**

1. Receive event via WebSocket
2. Attempt atomic Redis claim (SET NX)
3. If claim fails → exit (another instance owns it)
4. Process event
5. Persist result to database
6. If Database found eventid already exist return
7. Mark event as completed in Redis

<img width="500" height="1000" alt="image" src="https://github.com/user-attachments/assets/7fc15db5-e1ac-46fd-b83d-c0b1aed95be9" />

**How is Deduplication happens**

1. The muliple event came throught websoket (Assuming dupliates)
2. We try to claim each event id and store in redis cache (faster and reliable storage) using SET NX and TTL of 60 Seconds
3. Key: Event id
   value: "processing",
   expiry: 60 Sec,
   when: When.NotExists
4. This is where we prevent the race condition
5. If redis already found the event id in redis it will exit and process next event id
6. Else event id will pass for processing
7. When processing is complete event will be again saved to redis with completed TTL (Eg 1 hour)
8. This is to save from any transient error or crash happens in redis server.

Stale or abandoned claims are handled using time-bound locks.
Each claim is created atomically with a TTL.
If a worker crashes or fails to release the claim, Redis automatically expires it, allowing another worker to retry processing.
Retry and a
