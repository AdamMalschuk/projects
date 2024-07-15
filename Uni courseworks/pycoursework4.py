import math
import matplotlib.pyplot as plt
import numpy as np
import random
n = 1000#number of random numbers
maxval = 1000#maximum random value
minval = 100#minimum random value
X=np.empty(n,dtype=float)#creating empty array
Y=np.empty(n,dtype=float)#creating empty array
def walk(steps):
    xd=0
    yd=0
    for i in range (1,steps):#adds a random direction to the end of the path
        xd+=(random.random()-0.5)*2
        yd+=(random.random()-0.5)*2
        X[i]=xd
        Y[i]=yd
    return(X,Y)
for j in range (1,100):#creates 100 random length walks(between 100 and 1000 steps)
    X,Y=walk(random.randint(100,1000))
    plt.plot(X,Y)
plt.show()
