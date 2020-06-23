param
(
  $token
)
$ver = (gci "$env:userprofile\.nuget\packages\codecov").Name
$cmd = "$env:userprofile\.nuget\packages\codecov\$ver\tools\codecov.exe";
$fName = ".\YourProjectName.Tests\coverage.opencover.xml";
$arg1 = "-f ""$fName""";
$arg2 = "-t $token";
& $cmd $arg1 $arg2