import re
import csv

gFieldNames = ["EPC", "Antenna", "Count", "Time", "Protocol", "CRC", "PC"]
gEPCSet = set()

def createNewStorageTable( csvStorageFName ):
   with open(csvStorageFName, "w") as csvFile:
      writer = csv.DictWriter(csvFile, fieldnames=gFieldNames)
      writer.writeheader()

def storeNewValues( newValueFName, csvStorageFName ):
   with open(newValueFName, 'r') as txtIn:
      with open(csvStorageFName, "a+") as csvOut:
         for entries in txtIn:
            values = parseNewEntry(entries)
            if len(values) >= 7:
               writer = csv.DictWriter(csvOut, fieldnames=gFieldNames)
               writer.writerow({gFieldNames[0]:values[0], gFieldNames[1]:values[1], gFieldNames[2]:values[2], gFieldNames[3]:values[3], gFieldNames[4]:values[4], gFieldNames[5]:values[5], gFieldNames[6]:values[6]})
   return

def parseNewEntry( entry ):
   categories = re.split("(\s+)", entry)
   csvFields = []

   for phrases in categories:
      words = re.split("(:)", phrases)
      if verifyPhrase(words):
         csvFields.append(words[2])
   
   return csvFields

def verifyPhrase( words ):
   if len(words) < 3:
      return False
   elif words[1] != ":":
      return False
   return True

def getEPCList( csvStorageFName ):
   with open(csvStorageFName, "r") as csvStorageTable:
      reader = csv.DictReader(csvStorageTable, fieldnames=gFieldNames)
      epcs = []
      index = 0
      for row in reader:
         # skip the header!
         if index > 0:
            epcs.append(row["EPC"])
         index = index + 1
   
   return epcs

def createSetFromStorageTable( EPCList ):
   epc = set(EPCList)

# run the process
#storeNewValues("readOut.txt", "storageTable.csv")
#createNewStorageTable("storageTable.csv")
epcList = getEPCList("storageTable.csv")
createSetFromStorageTable(epcList)
gEPCSet.update(epcList)
print(gEPCSet)
print("Inventory count: ", len(gEPCSet))

