## Concept

This is a chat application which uses symmetric key encryption 
and public key cryptography for key exchange. Users available to add
new dialog with existing user and send messages to this dialog after
key exchange.

## RSA keys

Pubic keys of users stored in database at backend.
Private keys should be stored locally. For this purpose application
store plaintext files with `.xml` extension with name pattern: 
`{username}-private-key.xml` next to application binary. This file will
be created automatically for newly created users with Sign Up command.
For successful signing in it is not required to have private key. But
for creating new dialog and reading existing messages it is necessary.
See explanation below.

## AES key

Symmetric key can be restored from dialog history. To begin a new 
conversation between users **A**  and **B** (assuming that **A** 
initiate dialog) application send message contained AES generated AES 
key and encrypted by public key of user **B**. Then user **B** should
to send back to **A** decrypted AES key encrypted with **A** public key.
In this case both users can restore AES key from message history by its
own private keys.

## Demo

Private keys of existing users listed in table below available in 
this repository.

| Username | Password | Key file |
|---|---|---|
| agent-j | j_password | agent-j-private-key.xml |
| agent-k | k_password | agent-k-private-key.xml |
| agent-m | m_password | agent-m-private-key.xml |

## Logging

Log file `log.txt` placed next application binary.