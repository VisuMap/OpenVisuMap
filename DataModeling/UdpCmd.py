import sys, socket, struct
cmdPort = int(sys.argv[1])
cmd = sys.argv[2]

skt = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
skt.connect(('localhost', cmdPort))
skt.sendall(bytearray(struct.pack('i'+str(len(cmd))+'s', len(cmd), cmd.encode('utf-8'))))

