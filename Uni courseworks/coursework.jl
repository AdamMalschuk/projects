using Plots
function systemsetup()
    
    start = -10
    Uarray = [0.0,5.5,0.0,5.5,0.0,]
    regionwidths = [2.0,0.2,0.7,0.2,2.0]
    resolution = 100
    total = round(sum(regionwidths)*10)/10
    dx = total/resolution
    E = 0.5
    Mass = 9.109 * (10 ^ (-31))
    hbar = 1.054571817*10^(-34) * 6.2415*(10^18)
    
    reflection(start,Uarray,regionwidths,resolution,total,dx,E,Mass,hbar)
end
function reflection(start,Uarray,regionwidths,resolution,total,dx,E,Mass,hbar)
    energies = 10
    IPV = 1 #intervals per electron volt
    RP = Array{ComplexF64}(undef,energies,1)
    for c in 1:10
        E = c/IPV
        k = Array{ComplexF64}(undef,length(Uarray),1)
        regionpos = Array{Float64}(undef,length(Uarray),1)
        ab = Array{ComplexF64}(undef,length(Uarray),2)
        ab[length(Uarray),1] = 1
        ab[length(Uarray),2] = 0
        regionpos = regionposgen(regionwidths,start,regionpos)
        k = kcalc(Uarray,Mass,E,hbar,k)
        println(k)
        ab = ABgen(ab,Uarray,k,regionpos)
        RP[c] = ((abs(ab[1,2]/complex(ab[1,1])))^2)
    end
    x = Array{Float64}(undef,length(RP),1)
    for c in 1:length(RP)
        x[c] = c
    end
    println(RP)
    plot(x,real(RP))
end
function transmission(start,Uarray,regionwidths,resolution,total,dx,E,Mass,hbar)
    energies = 10
    IPV = 1 #intervals per electron volt
    TP = Array{ComplexF64}(undef,energies,1)
    for c in 1:10
        E = c/IPV
        k = Array{ComplexF64}(undef,length(Uarray),1)
        regionpos = Array{Float64}(undef,length(Uarray),1)
        println(length(regionpos))
        ab = Array{ComplexF64}(undef,length(Uarray),2)
        ab[length(Uarray),1] = 1
        ab[length(Uarray),2] = 0
        regionpos = regionposgen(regionwidths,start,regionpos)
        k = kcalc(Uarray,Mass,E,hbar,k)
        println(k)
        ab = ABgen(ab,Uarray,k,regionpos)
        TP[c] = ((abs(ab[length(Uarray),1]/complex(ab[1,1])))^2) * (k[length(k)]/k[1])
    end
    x = Array{Float64}(undef,length(TP),1)
    for c in 1:length(TP)
        x[c] = c
    end
    println(TP)
    plot(x,real(TP))
end
function plotwavefunction(start,Uarray,regionwidths,resolution,total,dx,E,Mass,hbar)
    
    k = Array{ComplexF64}(undef,length(Uarray),1)
    regionpos = Array{Float64}(undef,length(Uarray),1)
    ab = Array{ComplexF64}(undef,length(Uarray),2)
    ab[length(Uarray),1] = 1
    ab[length(Uarray),2] = 0
    wavefunction = Array{ComplexF64}(undef,resolution,1)
    x = Array{Float64}(undef,resolution,1)
    regionpos = regionposgen(regionwidths,start,regionpos)
    k = kcalc(Uarray,Mass,E,hbar,k)
    println(k)
    ab = ABgen(ab,Uarray,k,regionpos)
    println(ab)
    x = xgen(start,dx,resolution,x)
    wavefunction = wavefunctionf(x,k,ab,regionwidths,dx)
    plot(x, real(wavefunction))
end
function transfermatrix(k1,k2,xn,ab2)
    ab1 = Array{ComplexF64}(undef,2,1)
    c = 1/(2*k1)
    kp = (k1+k2)
    km = (k1-k2)
    T = [c*(kp*exp(complex((-im) * (xn * km)))) c*(km*exp(complex((-im)*xn*kp))); c*(km*exp(complex((im) * xn*kp))) c*(kp*exp(complex(im * xn*km)))]
    ab1[1] = (ab2[1]*T[1,1])+(ab2[2]*T[1,2])
    ab1[2] = (ab2[1]*T[2,1])+(ab2[2]*T[2,2])
    return(ab1)
end
function wavefunctionf(x,k,ab,regionwidths,dx)
    xc = 0
    wavefunction = Array{ComplexF64}(undef,resolution,1)
    for c in 1:length(regionwidths)
        for v in 1:round(regionwidths[c]/dx-1)
            xc = 1 + xc
            wavefunction[xc] = ab[c,1] * exp(im * k[c] * (x[Int(xc)])) + ab[c,2] * exp(-im * k[c] * (x[Int(xc)]))
        end
    end
    println(real(wavefunction))
    println(x)
    return wavefunction
    
end
function kcalc(Uarray,Mass,E,hbar,k)
    for c in 1:length(Uarray)
        k[c] = (sqrt(complex(2*Mass*(E-Uarray[c]))))/hbar
    end
    return k
end
function ABgen(ab,Uarray,k,regionpos)
    for c in 1:length(Uarray)-1
        r = length(Uarray) - c + 1
        k1 = k[r-1]
        k2 = k[r]
        println(ab[r,1],ab[r,2])
        ab2 = [ab[r,1],ab[r,2]]
        xn = regionpos[r]
        abt = Array{ComplexF64}(undef,2,1)
        abt = transfermatrix(k1,k2,xn,ab2)
        ab[r-1,1] = abt[1]
        ab[r-1,2] = abt[2]
    end
    return ab
end

function xgen(start,dx,resolution,x)
    for c in 1:resolution
        if c == 1
            x[c] = start + dx
        else
            x[c] = x[c-1] + dx
        end
    end
    return x
end
function regionposgen(regionwidths,start,regionpos)
    for c in 1:length(regionwidths)
        if c == 1
            regionpos[c] = start
        else
            regionpos[c] = regionpos[c-1] + regionwidths[c-1]
        end
    end
    return regionpos
end


systemsetup()