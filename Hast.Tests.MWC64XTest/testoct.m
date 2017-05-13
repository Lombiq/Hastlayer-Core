%octave script to calculate FFT and histogram of data generated with PRNG
f=fopen("bin/Release/prng.dat","r");
a=fread(f,Inf,"uint32");
figure(1)
semilogy(abs(fft(a(1024+(1:1024)))))
figure(2)
hist(a, 100)