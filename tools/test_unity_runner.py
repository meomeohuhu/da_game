"""Regression checks against false-positive Unity test reporting; no Unity mock pass."""
import tempfile
from pathlib import Path
import unittest
from run_unity_tests import read_result


class NUnitReportTests(unittest.TestCase):
    def check_xml(self, text, valid=False):
        with tempfile.TemporaryDirectory() as temp:
            p = Path(temp) / 'results.xml'
            p.write_text(text)
            if valid:
                self.assertEqual(4, read_result(p)['passed'])
            else:
                with self.assertRaises(ValueError):
                    read_result(p)

    def test_complete_pass(self):
        self.check_xml('<test-run result="Passed" total="4" passed="4" failed="0" skipped="0"/>', True)

    def test_zero_tests_cannot_pass(self):
        self.check_xml('<test-run result="Passed" total="0" passed="0"/>')

    def test_failed_tests_cannot_pass(self):
        self.check_xml('<test-run result="Failed" total="4" passed="3" failed="1"/>')

    def test_skipped_tests_cannot_pass(self):
        self.check_xml('<test-run result="Passed" total="4" passed="3" skipped="1"/>')

    def test_wrong_xml_cannot_pass(self):
        self.check_xml('<html>License activation required</html>')

    def test_incomplete_counts_cannot_pass(self):
        self.check_xml('<test-run result="Passed" total="4"/>')


if __name__ == '__main__':
    unittest.main()
