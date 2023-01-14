# JetBrains-Intern-Problem
This is my solution to the problem proposed by JetBrains Educational Team

Given a folder path, search recursively all txt files which match a given mask(regex) and for each of them determine:
which delimiter is used (comma, tab and semicolons are possible),
which digital (dot or comma) and thousand (dot, comma or space) separators are used for numbers,
which date format is used (DD/MM/YYYY, MM/DD/YYYY or YYYY/MM/DD are possible),

Create statistics for the combination of used delimiter, number and date format.

All files are processed multi-threaded and all results should be displayed in a single file.
