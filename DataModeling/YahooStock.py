import datetime as dt
import pandas as pd
import pandas_datareader.data as web

def CheckYahoo():
    start = dt.datetime(2000, 1, 1)
    end = dt.datetime(2018, 12,31)
    df = web.DataReader('IBM', 'yahoo', start, end)
    print(df.head())

def QuandlTest():
    start = dt.datetime(2000, 1, 1)
    end = dt.datetime(2018, 12,31)
    df = web.DataReader('WIKI/IBM', 'quandl', start, end)
    print(df.head())

QuandlTest()