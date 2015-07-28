var doc = fl.openDocument("{FILE_PATH_TO_OPEN}");
doc.publish();
doc.close();
FLfile.runCommandLine("{COMPLETE_COMMAND}");
