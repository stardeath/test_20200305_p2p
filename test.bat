rem this test file makes it easy to spawn 4 instances of the debug build
rem 3 "clients" and 1 "server"
rem client "0" and "1" with a connection to server "0" and client "2" connected to client "1"

start .\test_20200305_p2p\bin\Debug\test_20200305_p2p.exe "server0@127.0.0.1:5000"
start .\test_20200305_p2p\bin\Debug\test_20200305_p2p.exe "client0@127.0.0.1:5010" "server0@127.0.0.1:5000"
start .\test_20200305_p2p\bin\Debug\test_20200305_p2p.exe "client1@127.0.0.1:5011" "server0@127.0.0.1:5000"
start .\test_20200305_p2p\bin\Debug\test_20200305_p2p.exe "client2@127.0.0.1:5012" "client1@127.0.0.1:5011"
