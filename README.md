# rx-dojo

## Error
Unhandled Exception: Microsoft.Azure.ServiceBus.SessionCannotBeLockedException: The requested session 'session' cannot be accepted. It may be locked by another receiver. 

### example 1

10 messages, 5 mins lock timeout
foreach 90s -> all success. 

### example 2
10 messages, 5 mins lock timeout
foreach 5s, one 5min 10s -> the one SessionLockLostException

### example 3
10 messages, 2 mins lock timeout
foreach 5s, one 2min 10s -> the one SessionLockLostException & rest messages are SessionLockLostException 

### example 4
10 messages, 2 mins lock timeout
foreach 5s, one 2min 10s, re-create session and receive 10 message -> all complete

### example 5
10 messages, 2 mins lock timeout
foreach 5s, one 2min 10s, renewLockSession -> rest message SessionLockLostException

