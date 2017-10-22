import re

readF = "readOut.txt"
csvF = "storageTable.csv"
with open(readF, 'r') as f:
   csvOut = open(csvF, "a+")
   for line in f:
      words = re.split("(\s+)", line)
      csvStr = ""
      for w in words:
         phrases = re.split("(:)", w)
         if len(phrases) >= 2:
            csvStr += phrases[2]
            csvStr += ","
      if csvStr != "":
         csvStr += "\n"
         csvOut.write(csvStr)
f.close()
csvOut.close()