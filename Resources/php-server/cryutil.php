<?php
  $nullhack = null;
  
  function &array_hack($array) { return $array; }
  
	function &pth_new_array($size) {
    $output = array();
    while ($size-- > 0) array_push($output, null);
    return $output;
  }
  
  function &pth_dictionary_get_keys(&$dictionary) {
    $output = array();
    foreach ($dictionary as $key => $ignored) {
      array_push($output, $key);
    }
    return $output;
  }
  
  $pth_program_data = null;
  function pth_set_program_data(&$pd) {
    global $pth_program_data;
    $pth_program_data = &$pd;
  }
  
  function &pth_getprogramdata() {
    global $pth_program_data;
    return $pth_program_data;
  }
?>