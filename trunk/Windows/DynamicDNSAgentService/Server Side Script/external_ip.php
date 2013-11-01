<?php header('Content-Type: text/xml');
print '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>';
echo("<!-- For use with DynamicDNS Agent Service -->");
echo("<ip><address>".$_SERVER["REMOTE_ADDR"]."</address></ip>");
?>
