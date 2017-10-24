import re

def storeNewValues( newValueFName, csvStorageFName ):
   """ stores properly formatted values from the newValueFName file into the csvStorageFName file.
   Assumes that newValueFName is a text file output by the thingmagic library. Assumes that csvStorageFName
   is a csv file.

   """
   txtIn = open(newValueFName, 'r')
   csvOut = open(csvStorageFName, "a+")

   for entries in txtIn:
      csvOutStr = parseNewEntry(entries)
      csvOut.write(csvOutStr)

   txtIn.close()
   csvOut.close()
   return


def parseNewEntry( entry ):
   """ parses a line from a text file (an entry) and returns a CSV formatted string of the various phrases in the entry """
   categories = re.split("(\s+)", entry)
   csvStr = ""

   for phrases in categories:
      words = re.split("(:)", phrases)
      if verifyPhrase(words):
         csvStr += words[2]
         csvStr += ","
   
   csvStr += "\n"
   return csvStr

def verifyPhrase( words ):
   """ verifies that a phrase in an entry is in the correct format which is [title]:[value]
   returns true if the format is correct and false otherwise

   """
   if len(words) != 3:
      return False
   elif words[1] != ":":
      return False
   return True

# run the process
storeNewValues("readOut.txt", "storageTable.csv")