<?php
  $nullhack = null;
  
	function &pth_new_array($size) {
    $output = array();
    while ($size-- > 0) array_push($output, null);
    return $output;
  }
?>